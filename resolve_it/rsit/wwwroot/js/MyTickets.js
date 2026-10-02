document.addEventListener("DOMContentLoaded", () => {
    const ticketTable = document.querySelector(".rit-ticket-table");

    if (!ticketTable) {
        return;
    }

    /*
     * Keep table rows keyboard accessible.
     * The actual navigation remains handled by the
     * Razor-generated Details links.
     */
    const viewButtons =
        ticketTable.querySelectorAll(".rit-view-btn");

    viewButtons.forEach((button) => {
        button.addEventListener("keydown", (event) => {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                button.click();
            }
        });
    });
});