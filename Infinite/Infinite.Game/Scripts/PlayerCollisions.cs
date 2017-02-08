using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Scripts
{
    public class PlayerCollisions : AsyncScript
    {
        public override async Task Execute()
        {
            var character = Entity.Get<CharacterComponent>();

            while (Game.IsRunning)
            {
                await character.NewCollision();
                if (character.IsGrounded)
                    PlayerController.Jumps = 0;
            }
        }
    }
}
