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
    let openToggle = document.getElementById('option_a1');
    let archiveToggle = document.getElementById('option_a2');
    let allToggle = document.getElementById('option_a3');

    let openTickets = document.getElementById('openTickets');
    let archivedTickets = document.getElementById('archivedTickets');
    let allTickets = document.getElementById('allTickets');

    if (openToggle.classList.contains('active')) {
        $(openTickets).show();
        $(archivedTickets).hide();
        $(allTickets).hide();
    }

    if (archiveToggle.classList.contains('active')) {
        $(openTickets).hide();
        $(archivedTickets).show();
        $(allTickets).hide();
    }

    if (allToggle.classList.contains('active')) {
        $(openTickets).hide();
        $(archivedTickets).hide();
        $(allTickets).show();
    }
}

