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