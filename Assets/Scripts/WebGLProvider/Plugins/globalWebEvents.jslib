mergeInto(LibraryManager.library, {
    
    registerVisibilityChangeEvent: function () {
        document.addEventListener("visibilitychange", function () {
            SendMessage("WebGLProvider", "OnVisibilityChange", document.visibilityState);
        });
        
        if (document.visibilityState != "visible")
            SendMessage("WebGLProvider", "OnVisibilityChange", document.visibilityState);
    },
});