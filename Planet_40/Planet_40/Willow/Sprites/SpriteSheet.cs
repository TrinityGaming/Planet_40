using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using System.IO;
using System.Xml.Serialization;



namespace Willow.Sprites
{
  [Serializable]
  /// <summary>
  /// Defines information about a single sprite element within the SpriteSheet.
  /// </summary>
  public class SpriteDefinition
  {
    /// <summary>
    /// Name of the sprite
    /// </summary>
    public string Name;

    /// <summary>
    /// Bounds of the sprite within the SpriteSheet texture.
    /// </summary>
    public Rectangle Bounds;

    [XmlIgnore]
    /// <summary>
    /// Reference to the SpriteSheet texture.
    /// </summary>
    public Texture2D Texture;

    /// <summary>
    /// Construct a SpriteDefinition instance.
    /// </summary>
    public SpriteDefinition()
    {
    }

    /// <summary>
    /// Construct a SpriteDefinition instance.
    /// </summary>
    /// <param name="name">Sprite name.</param>
    /// <param name="bounds">Bounds within the SpriteSheet.</param>
    /// <param name="texture">Reference to SpriteSheet texture.</param>
    public SpriteDefinition(string name, Rectangle bounds, Texture2D texture)
    {
      Name = name;
      Bounds = bounds;
      Texture = texture;
    }
  }


  /// <summary>
  /// Provides functionality for creating and utilizing a sprite sheet.
  /// </summary>
  public class SpriteSheet : IDisposable
  {
    private Texture2D texture;
    private Dictionary<string, SpriteDefinition> sprites;

    /// <summary>
    /// SpriteSheet texture reference.
    /// </summary>
    public Texture2D Texture { get { return texture; } }

    /// <summary>
    /// Dictionary containing a SpriteDefinition instance for each sprite in the sprite sheet.
    /// </summary>
    public Dictionary<string, SpriteDefinition> Sprites { get { return sprites; } }

    /// <summary>
    /// Default property, returning the SpriteDefinition instance for the provided sprite name.
    /// </summary>
    /// <param name="index">Sprite name.</param>
    /// <returns></returns>
    public SpriteDefinition this[string index] { get { return sprites[index]; } }

    /// <summary>
    /// Create a SpriteSheet instance.
    /// </summary>
    public SpriteSheet()
    {
    }


    /// <summary>
    /// Load sprite sheet data.
    /// </summary>
    /// <param name="content">ContentManager instance</param>
    /// <param name="fileName">Name of the XML file containing data for each SpriteDefinition.</param>
    /// <param name="resourceName">Name for the sprite sheet texture.</param>
    public void Load(ContentManager content, string fileName, string resourceName)
    {
      // load the texture
      texture = content.Load<Texture2D>(resourceName);

      // open stream for reading
      Stream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

      // deserialize the sprite definition dictionary
      XmlSerializer serializer = new XmlSerializer(typeof(List<SpriteDefinition>));
      List<SpriteDefinition> spriteList = (List<SpriteDefinition>)serializer.Deserialize(reader);

      // add sprites from list to dictionary
      sprites = new Dictionary<string, SpriteDefinition>();
      foreach (SpriteDefinition sprite in spriteList)
      {
        sprite.Texture = texture;
        sprites.Add(sprite.Name, sprite);
      }


      // close the stream
      reader.Close();
    }


    /// <summary>
    /// Load externally created SpriteDefinition instances, e.g. from a sprite sheet creator application.
    /// </summary>
    /// <param name="texture">Name for the sprite sheet texture.</param>
    /// <param name="sprites">List of SpriteDefinition instances.</param>
    public void Load(Texture2D texture, List<SpriteDefinition> sprites)
    {
      this.texture = texture;
      this.sprites = new Dictionary<string, SpriteDefinition>();

      for (int i = 0; i < sprites.Count; i++)
      {
        SpriteDefinition sprite = sprites[i];
        this.sprites.Add(sprite.Name, new SpriteDefinition(sprite.Name, sprite.Bounds, texture));
      }
    }


    /// <summary>
    /// Save sprite sheet information.
    /// </summary>
    /// <param name="xmlFileName">Name of the XML file that will contain data for each SpriteDefinition.</param>
    /// <param name="textureFileName">Name of the texture that will contain the sprite sheet.</param>
    /// <param name="format">Image format.</param>
    /// 
#if !XBOX
    /*
    public void Save(string xmlFileName, string textureFileName, ImageFileFormat format)
    {
      // save the actual texture image
      texture.Save(textureFileName, format);

      // open stream for writing
      Stream writer = File.Open(xmlFileName, FileMode.Create);

      // serialize the sprite definition dictionary
      XmlSerializer serializer = new XmlSerializer(typeof(List<SpriteDefinition>));

      // dictionary can't be serialized so copy the sprites to a list
      List<SpriteDefinition> spriteList = new List<SpriteDefinition>(sprites.Count);
      foreach (SpriteDefinition sprite in sprites.Values)
        spriteList.Add(sprite);

      serializer.Serialize(writer, spriteList);


      // close the stream
      writer.Close();
    }
     */
#endif


    /// <summary>
    /// Dispose the texture
    /// </summary>
    public void Dispose()
    {
      if (texture != null)
        texture.Dispose();
      texture = null;
    }
  }
}
