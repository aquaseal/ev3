$(function () {
    var client = new WindowsAzure.MobileServiceClient('https://ev3controller.azure-mobile.net/', 'icyMyejcoKJpeywMFMqElDfqJCFGTs49'),
        ev3CommandsTable = client.getTable('ev3Commands');

    function handleError(error) {
        var text = error + (error.request ? ' - ' + error.request.status : '');
        $('#errorlog').append($('<li>').text(text));
    }

    // command events
    $(".ev3CMD").click(function (event) {
        var cmd = $(this).attr("data-command");
        ev3CommandsTable.insert({ cmd: cmd }).then(refreshCommandList, handleError);
    });

    // manual refresh
    $(".ev3CMDListRefresh").click(function (event) {
        refreshCommandList();
    });

    // delete
    $(".ev3DeleteCMDList").click(function (event) {
        var query = ev3CommandsTable.select("id").read().done(function (results) {
            for (var i = 0; i < results.length; i++) {
                ev3CommandsTable.del({ id: results[i].id });
            }

            $('#commandList').empty();
            $('#summary').html('<strong>' + 0 + '</strong> item(s)');
        }, handleError)
    });

    // list all commands
    function refreshCommandList() {
        //var query = ev3CommandsTable.where({ executed: false });
        var query = ev3CommandsTable;

        query.read().then(function (commands) {
            var listItems = $.map(commands, function (item) {
                return $('<li>')
                    .append($('<div class="command-text">').append(item.cmd));

            });

            $('#commandList').empty().append(listItems).toggle(listItems.length > 0);
            $('#summary').html('<strong>' + commands.length + '</strong> item(s)');
        }, handleError);

    }

    refreshCommandList();
});