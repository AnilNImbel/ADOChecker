document.addEventListener('DOMContentLoaded', () => {
    const menuItems = document.querySelectorAll('.sidebar ul li');
    const sections = document.querySelectorAll('.content');

    menuItems.forEach(item => {
        item.addEventListener('click', () => {
            // Remove active class from all menu items
            menuItems.forEach(i => i.classList.remove('active'));
            // Add active class to clicked menu item
            item.classList.add('active');

            // Hide all sections
            sections.forEach(section => section.style.display = 'none');

            // Show the selected section
            const sectionId = item.getAttribute('data-section');
            const targetSection = document.getElementById(sectionId);
            if (targetSection) {
                targetSection.style.display = 'block';
            }
        });
    });
});

function toggleSidebar() {

    const sidebar = document.getElementById("sidebar");
    const content = document.querySelector('.content')
    sidebar.classList.toggle("collapsed");

    if (sidebar.classList.contains("collapsed")) {
        content.style.marginLeft = "100px";
    } else {
        content.style.marginLeft = "250px";
    }
}


function validateDateRange() {
    const fromDateInput = document.getElementById("fromDate");
    const toDateInput = document.getElementById("toDate");

    if (!fromDateInput.value || !toDateInput.value) {
        alert("Please select both From and To dates.");
        return false;
    }

    const fromDate = new Date(fromDateInput.value);
    const toDate = new Date(toDateInput.value);

    if (isNaN(fromDate) || isNaN(toDate)) {
        alert("Invalid date format.");
        return false;
    }

    if (fromDate > toDate) {
        alert("From Date cannot be greater than To Date.");
        return false;
    }

    // Show loader, hide result and no-records message
    document.getElementById("loader").style.display = "block";
    document.getElementById("result").style.display = "none";
    document.getElementById("noRecordsMessage").style.display = "none";

    return true;
}

document.querySelectorAll('.download-csv').forEach(button => {
    button.addEventListener('click', function () {
        const runId = this.getAttribute('data-run-id');
        const loader = this.nextElementSibling;

        this.disabled = true;
        loader.style.display = 'inline-block';

        fetch(`/Reports/DownloadCsv?runId=${runId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Failed to download CSV");
                }
                return response.blob();
            })
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `report_${runId}.csv`;
                document.body.appendChild(a);
                a.click();
                a.remove();
            })
            .catch(err => alert("Error: " + err.message))
            .finally(() => {
                loader.style.display = 'none';
                this.disabled = false;
            });
    });
});

$(document).on('change', '#sprintDashBoardFilter', function () {
    var selected = this.value;
    window.location.href = '/Dashboard/Index?selectedSprint=' + selected;
});

$(document).on('change', '#pipelineFilter', function () {
    let definitionId = '';
    $('#workItemsTable').html("");
    $("#loader").show();
    definitionId = this.value;
    $.ajax({
        url: '/Build/GetBuildDetails',
        type: 'GET',
        data: { definitionId: definitionId },
        success: function (result) {
            $('#workItemsTable').html(result);
        },
        error: function () {
            alert('Error loading partial view.');
        },
        complete: function () {
            // Hide the loader after the request completes
            $("#loader").hide();
        }
    });
});

$('.loadData').click(function () {
    let currentWorkType = '';
    $('#workItemsTable').html("");
    $("#loader").show();
    currentWorkType = $(this).attr('name');
    $.ajax({
        url: '/DashBoard/GridLoad',
        type: 'GET',
        data: { missingType: currentWorkType },
        success: function (result) {
            $('#workItemsTable').html(result);
        },
        error: function () {
            alert('Error loading partial view.');
        },
        complete: function () {
            // Hide the loader after the request completes
            $("#loader").hide();
        }
    });
});

$('.loadData').click(function () {
    let currentWorkType = '';
    $('#workItemsTable').html("");
    $("#loader").show();
    currentWorkType = $(this).attr('name');
    $.ajax({
        url: '/DashBoard/GridLoad',
        type: 'GET',
        data: { missingType: currentWorkType },
        success: function (result) {
            $('#workItemsTable').html(result);
        },
        error: function () {
            alert('Error loading partial view.');
        },
        complete: function () {
            // Hide the loader after the request completes
            $("#loader").hide();
        }
    });
});