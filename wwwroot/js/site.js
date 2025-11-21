// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// signalr connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/claimHub")
    .configureLogging(signalR.LogLevel.Warning)
    .build();

connection.on("ClaimUpdated", function (claimId, newStatus) {
    // update rows in both lecturer and admin tables
    const row = document.querySelector(`#row-${claimId}`);
    if (row) {
        const statusEl = row.querySelector(".status");
        if (statusEl) statusEl.textContent = newStatus;
    }
    const adminRow = document.querySelector(`#adminRow-${claimId}`);
    if (adminRow) {
        const statusEl = adminRow.querySelector(".status");
        if (statusEl) statusEl.textContent = newStatus;
    }
});

connection.start().catch(err => console.error(err.toString()));

async function changeStatus(id, actionType, button) {
    try {
        button.disabled = true;
        const res = await fetch("/Admin/ChangeStatus", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id: id, actionType: actionType })
        });
        if (res.ok) {
            // server will push SignalR update; optionally display simple feedback
            console.log("Status changed");
        } else {
            const txt = await res.text();
            alert("Failed: " + txt);
        }
    } catch (e) {
        alert("Error changing status: " + e.message);
    } finally {
        button.disabled = false;
    }
}
