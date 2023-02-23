mergeInto(LibraryManager.library, {
  InitPurchases: function() {
    initPayments();
  },

  Purchase: function(id) {
    buy(UTF8ToString(id));
  },

  AuthenticateUser: function(direct) {
    var directString = UTF8ToString(direct);
    auth(directString);
  },

  GetUserData: function() {
    getUserData();
  },

  ShowFullscreenAd: function () {
    showFullscrenAd();
  },

  ShowRewardedAd: function(placement) {
    var placementString = UTF8ToString(placement);
    showRewardedAd(placementString);
    return placementString;
  },

  SetLeaderboard: function (leaderboardName, score){
    var leaderboardNameString = UTF8ToString(leaderboardName);
    var scoreString = UTF8ToString(score);
    setLeaderboard(leaderboardNameString, scoreString);
  },

  GetLeaderboard: function (leaderboardName){
    var leaderboardNameString = UTF8ToString(leaderboardName);
    getLeaderboardScore(leaderboardNameString);
  },

  CopyClipboard: function (text){
    var clipboard = UTF8ToString(text);
    copyToClipboard(clipboard);
  },

  OpenWindow: function(link){
    var url = Pointer_stringify(link);
      document.onmouseup = function()
      {
        window.open(url);
        document.onmouseup = null;
      }
  }
});