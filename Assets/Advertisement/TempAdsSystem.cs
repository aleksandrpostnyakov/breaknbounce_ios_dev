using System.Threading.Tasks;

namespace Advertusement
{
    public class TempAdsSystem : BaseAdsSystem
    {
        protected override string GameId { get; }
        protected override string AdsName { get; }
        public override Task<bool> ShowAdsForWinScreen()
        {
            throw new System.NotImplementedException();
        }

        public override bool ShowInterstitial()
        {
            throw new System.NotImplementedException();
        }
    }
}