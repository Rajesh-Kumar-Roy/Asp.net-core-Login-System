function ConfimDelete(uniqeId, isDeleteClicked) {
    var deletespan = 'deleteSpan_' + uniqeId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqeId;
    if (isDeleteClicked) {
        $('#' + deletespan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deletespan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}