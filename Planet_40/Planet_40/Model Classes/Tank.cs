using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using Willow;
using Willow.Input;
using Willow.Camera;


namespace Planet
{
    class Tank
    {
        // The radius of the tank's wheels. This is used when we calculate how fast they
        // should be rotating as the tank moves.
        const float TankWheelRadius = 18;

        public TerrainNode NodeUnderTank;
        public Movement Movement { get { return movement; } }


        // The tank's model - a fearsome sight.
        Model model;
        //Effect maskEffect;

        // we'll use this value when making the wheels roll. It's calculated based on 
        // the distance moved.
        Matrix wheelRollMatrix = Matrix.Identity;

        // The Simple Animation Sample at creators.xna.com explains the technique that 
        // we will be using in order to roll the tanks wheels. In this technique, we
        // will keep track of the ModelBones that control the wheels, and will manually
        // set their transforms. These next eight fields will be used for this
        // technique.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;

        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;

        Movement movement;

        public Vector3[] Triangle;




        /// <summary>
        /// Called when the Game is loading its content. Pass in a ContentManager so the
        /// tank can load its model.
        /// </summary>
        public void LoadContent(Game game)
        {
            model = game.Content.Load<Model>(@"Models\Tank");

            // as discussed in the Simple Animation Sample, we'll look up the bones
            // that control the wheels.
            leftBackWheelBone = model.Bones["l_back_wheel_geo"];
            rightBackWheelBone = model.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = model.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = model.Bones["r_front_wheel_geo"];

            // Also, we'll store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;

            movement = new Movement(game);
        }




         public void HandleInput(GameTime gameTime, Sphere planet, IInputManager inputManager)
        {
          double height;
          Vector3 normal;

          Triangle = null;

          Position3 oldPosition = movement.fLocalPosition;
          movement.Update(gameTime);

          NodeUnderTank = planet.FindNodeUnderPosition(movement.Position - planet.Position, out Triangle, out height, out normal);

          if (NodeUnderTank == null)
          {
            if (Globals.Game.Graphics.IsFullScreen)
              Globals.Game.Graphics.ToggleFullScreen();
            throw new Exception("Unable to find node under tank.");
          }
          else
          {
            Position3 p = movement.Position - planet.Position;
            p.Normalize();
            movement.Position = planet.Position + (p * height);
          }


           movement.OrientUp(normal);


          // now we need to roll the tank's wheels "forward." to do this, we'll
          // calculate how far they have rolled, and from there calculate how much
          // they must have rotated.
          float distanceMoved = (float)Position3.Distance(oldPosition, movement.fLocalPosition);
          float theta = distanceMoved / TankWheelRadius * 1000.0f;
          int rollDirection = 0;

          Vector3 n0 = movement.fLinearVelocity;
          n0.Normalize();

          Vector3 n1 = movement.fForward;

           if (Vector3.Dot(n0, n1) < 0)
             rollDirection = 1;
           else
             rollDirection = -1;

           wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);
        }


         public void DrawMask(Sphere planet, Camera camera)
         {
           // Apply matrices to the relevant bones, as discussed in the Simple 
           // Animation Sample.
           leftBackWheelBone.Transform = wheelRollMatrix * leftBackWheelTransform;
           rightBackWheelBone.Transform = wheelRollMatrix * rightBackWheelTransform;
           leftFrontWheelBone.Transform = wheelRollMatrix * leftFrontWheelTransform;
           rightFrontWheelBone.Transform = wheelRollMatrix * rightFrontWheelTransform;

           // now that we've updated the wheels' transforms, we can create an array
           // of absolute transforms for all of the bones, and then use it to draw.
           Matrix[] boneTransforms = new Matrix[model.Bones.Count];
           model.CopyAbsoluteBoneTransformsTo(boneTransforms);


           // get world space position by adding terrain node position to planet position
           Position3 worldSpacePosition = movement.Position;

           // translate to camera space by subtracting the camera position, scale by planet scale
           Position3 cameraSpacePosition = (worldSpacePosition - camera.Position) * planet.Scale;

           Matrix cameraSpaceMatrix = planet.ScaleMatrix * movement.WorldMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);

           Vector3 lightDirection = (planet.Position - Position3.Zero).AsVector3;
           lightDirection.Normalize();

           foreach (ModelMesh mesh in model.Meshes)
           {
             foreach (BasicEffect effect in mesh.Effects)
             {
               effect.World = boneTransforms[mesh.ParentBone.Index] * cameraSpaceMatrix;
               effect.View = camera.ViewMatrix;
               effect.Projection = camera.ProjectionMatrix;
               effect.LightingEnabled = false;
               effect.PreferPerPixelLighting = false;
               effect.DiffuseColor = new Vector3(0);
               effect.VertexColorEnabled = true;
             }
             mesh.Draw();
           }
         }


        public void Draw(Sphere planet, Camera camera)
        {
          // Apply matrices to the relevant bones, as discussed in the Simple 
          // Animation Sample.
          leftBackWheelBone.Transform = wheelRollMatrix * leftBackWheelTransform;
          rightBackWheelBone.Transform = wheelRollMatrix * rightBackWheelTransform;
          leftFrontWheelBone.Transform = wheelRollMatrix * leftFrontWheelTransform;
          rightFrontWheelBone.Transform = wheelRollMatrix * rightFrontWheelTransform;

          // now that we've updated the wheels' transforms, we can create an array
          // of absolute transforms for all of the bones, and then use it to draw.
          Matrix[] boneTransforms = new Matrix[model.Bones.Count];
          model.CopyAbsoluteBoneTransformsTo(boneTransforms);


          // get world space position by adding terrain node position to planet position
          Position3 worldSpacePosition = movement.Position;

          // translate to camera space by subtracting the camera position, scale by planet scale
          Position3 cameraSpacePosition = (worldSpacePosition - camera.Position) * planet.Scale;

          Matrix cameraSpaceMatrix = planet.ScaleMatrix * movement.WorldMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);

          Vector3 lightDirection = (planet.Position - Position3.Zero).AsVector3;
          lightDirection.Normalize();

          foreach (ModelMesh mesh in model.Meshes)
          {
            foreach (BasicEffect effect in mesh.Effects)
            {
              effect.World = boneTransforms[mesh.ParentBone.Index] * cameraSpaceMatrix;
              effect.View = camera.ViewMatrix;
              effect.Projection = camera.ProjectionMatrix;
              effect.DirectionalLight0.Direction = -lightDirection;
              effect.EnableDefaultLighting();
              effect.VertexColorEnabled = false;
              effect.PreferPerPixelLighting = true;
            }
            mesh.Draw();
          }
        }

    }
}
