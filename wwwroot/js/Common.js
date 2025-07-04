﻿document.addEventListener('DOMContentLoaded', () => {
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
        content.style.marginLeft = "95px";
    } else {
        content.style.marginLeft = "245px";
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

document.addEventListener("click", function (e) {
    if (e.target.getAttribute("projectZero") === "Missing") {
        var Missing = "Missing";
        var Updated = "Updated";
        const element = e.target;

        const attributes = ["why1", "why2", "why3", "owner"];

        attributes.forEach(attr => {
            const attrValue = element.getAttribute(attr);
            const inputElement = document.getElementById(attr);

            if (inputElement) {
                if (!attrValue) {
                    inputElement.classList.add(Missing);
                    inputElement.innerText = Missing;
                } else {
                    inputElement.classList.add(Updated);
                    inputElement.innerText = Updated;
                }
            }
        });

        const modal = new bootstrap.Modal(document.getElementById('projectZeroModal'));
        modal.show();
    }

    if (e.target.getAttribute("prlifeCycle") === "Missing") {
        var Missing = "Missing";
        var Updated = "Updated";
        const element = e.target;

        const attributes = ["demo", "signedOff", "unitTest", "analysisHours", "actualHours"];

        attributes.forEach(attr => {
            const attrValue = element.getAttribute(attr);
            const inputElement = document.getElementById(attr);

            if (inputElement) {
                if (!attrValue) {
                    inputElement.classList.add(Missing);
                    inputElement.innerText = Missing;
                } else {
                    inputElement.classList.add(Updated);
                    inputElement.innerText = Updated;
                }
            }
        });

        const modal = new bootstrap.Modal(document.getElementById('prLifeCycleModal'));
        modal.show();
    }
});

function toggleTooltip(element) {
    element.classList.toggle("show");
}

// Optional: Hide tooltip when clicking outside
document.addEventListener("click", function (event) {
    const tooltip = document.querySelector(".tooltip-container");
    if (tooltip && !tooltip.contains(event.target)) {
        tooltip.classList.remove("show");
    }
});
