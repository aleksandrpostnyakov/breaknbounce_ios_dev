mergeInto(LibraryManager.library, {

    VkShowPayment: function(itemId) {
        showPayment(UTF8ToString(itemId));
    },

    VkShowRewarderAd: function() {
        showRewardedAd();
    },

    VkShowInterstitialAd: function() {
        showInterstitialAd();
    },

    VkAddToFavorites: function() {
        addToFavorites();
    },

    VkShowInviteBox: function() {
        showInviteBox();
    },

    VkAuth: function() {
        auth();
    },

    VkShowWallPost: function(id, data) {
        showWallPost(id, data);
    },

    VkCopyClipboard: function (text){
        var clipboard = UTF8ToString(text);
        copyToClipboard(clipboard);
    },
});