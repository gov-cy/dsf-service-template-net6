// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    if (window.history.replaceState) {
        //prevents browser from storing history with each change:
        window.history.replaceState(statedata, title, url);
    }

});