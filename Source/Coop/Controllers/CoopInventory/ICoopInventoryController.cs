using Comfort.Common;
using JetBrains.Annotations;
using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.CoopInventory
{
    public interface ICoopInventoryController
    {
        public void Execute(AbstractInventoryOperation operation, [CanBeNull] Callback callback);

        public string GetMongoId();

    }
}
