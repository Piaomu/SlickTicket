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

console.log("loaded")

let openToggle = document.getElementById('openToggle');
let archiveToggle = document.getElementById('archiveToggle');
let allToggle = document.getElementById('allToggle');

ticketToggle();

openToggle.addEventListener('click', () => {
    $("#openTickets").show();
    $("#archivedTickets").hide();
    $("#allTickets").hide();
})

archiveToggle.addEventListener('click', () => {
    $("#openTickets").hide();
    $("#archivedTickets").show();
    $("#allTickets").hide();
})

allToggle.addEventListener('click', () => {
    $("#openTickets").hide();
    $("#archivedTickets").hide();
    $("#allTickets").show();
})

function ticketToggle() {

    $("#openTickets").show();
    $("#archivedTickets").hide();
    $("#allTickets").hide();
}

