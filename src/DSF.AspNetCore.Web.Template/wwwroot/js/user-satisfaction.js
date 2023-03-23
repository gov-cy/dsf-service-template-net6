$(document).ready(function () {
    $('#btnUserSatisfactionSubmit').click(function (e) {
        $(this).prop('disabled', true);
        $('#userSatisfactionForm').submit();
    });
});