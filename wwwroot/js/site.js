//Handles in-line deletion
function confirmDelete(uniqueId, isDeleteClicked) {
    var deleteSpan = "deleteSpan_" + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}

function ticketToggle() {
    let openToggle = document.getElementById('openToggle');
    let archiveToggle = document.getElementById('archiveToggle');
    let allToggle = document.getElementById('allToggle');

    let openTickets = document.getElementById('openTickets');
    let archivedTickets = document.getElementById('archivedTickets');
    let allTickets = document.getElementById('allTickets');

    if (openToggle.classList.contains('active')) {
        $("#openTickets").show();
        $("#archivedTickets").hide();
        $("#allTickets").hide();
    }

    if (archiveToggle.classList.contains('active')) {
        $("#openTickets").hide();
        $("#archivedTickets").show();
        $("#allTickets").hide();
    }

    if (allToggle.classList.contains('active')) {
        $("#openTickets").hide();
        $("#archivedTickets").hide();
        $("#allTickets").show();
    }
}

