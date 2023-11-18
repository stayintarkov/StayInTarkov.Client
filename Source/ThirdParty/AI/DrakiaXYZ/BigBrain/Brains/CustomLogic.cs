using EFT;
using System.Text;

namespace DrakiaXYZ.BigBrain.Brains
{
    public abstract class CustomLogic
    {
        public BotOwner BotOwner { get; private set; }

        public CustomLogic(BotOwner botOwner)
        {
            BotOwner = botOwner;
        }

        public virtual void Start() { }
        public virtual void Stop() { }

        public abstract void Update();

        public virtual void BuildDebugText(StringBuilder stringBuilder) { }
    }
}
