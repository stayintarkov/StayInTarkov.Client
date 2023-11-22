using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System;

namespace DrakiaXYZ.BigBrain.Internal
{
    internal class CustomLogicWrapper : BaseNodeClass
    {
        private CustomLogic customLogic;

        public CustomLogicWrapper(Type logicType, BotOwner bot) : base(bot)
        {
            customLogic = (CustomLogic)Activator.CreateInstance(logicType, new object[] { bot });
        }

        public override void Update()
        {
            customLogic.Update();
        }

        public void Start()
        {
            customLogic.Start();
        }

        public void Stop()
        {
            customLogic.Stop();
        }

        internal CustomLogic CustomLogic()
        {
            return customLogic;
        }
    }
}
