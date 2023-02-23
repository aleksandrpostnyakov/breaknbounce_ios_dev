mergeInto(LibraryManager.library, {

    OkShowPayment: function(name, description, code, price) {
        var nameString = UTF8ToString(name);
        var descriptionString = UTF8ToString(description);
        var codeString = UTF8ToString(code);
        
        showPayment(nameString, descriptionString, codeString, price);
    },

    OkLoadRewardedAd: function() {
        loadRewardedAd();
    },

    OkShowRewardedAd: function() {
        showRewardedAd();
    },

    OkShowInterstitialAd: function() {
        showInterstitialAd();
    },

    OkInit: function() {
        initOK();
    },

    CopyClipboard: function (text){
        var clipboard = UTF8ToString(text);
        copyToClipboard(clipboard);
    },
});