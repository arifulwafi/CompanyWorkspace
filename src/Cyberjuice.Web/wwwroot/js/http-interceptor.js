/**
 * Company HTTP Interceptor
 * Automatically includes the Company id in API requests
 */
(function () {
    'use strict';

    const CONFIG = window.WORKSPACE_CONSTANTS || {
        STORAGE_KEY: 'selectedWorkspaceId',
        HEADER_NAME: 'X-Company-Id'
    };

    function getWorkspaceId() {
        return localStorage.getItem(CONFIG.STORAGE_KEY);
    }

    function initJQueryInterceptor() {
        if (!window.jQuery) return;

        jQuery(document).ajaxSend(function (event, xhr, settings) {
            const workspaceId = getWorkspaceId();

            if (workspaceId) {
                xhr.setRequestHeader(CONFIG.HEADER_NAME, workspaceId);
            }
        });

    }

    function initialize() {
        initJQueryInterceptor();
    }

    // Trigger initialization immediately and on key events
    initialize();

    document.addEventListener('abp.dynamicScriptsInitialized', initialize);
    document.addEventListener('DOMContentLoaded', initialize);
    window.addEventListener('load', initialize);

})();
