const FacId = window.FacultyId;

async function fetchData(url) {
    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
        return await res.json();
    } catch (err) {
        console.error('❌ Error fetching data:', err);
        return null;
    }
}

function getColor(index) {
    const palette = [
        '#4e79a7', '#f28e2b', '#e15759', '#76b7b2',
        '#59a14f', '#edc948', '#b07aa1', '#ff9da7',
        '#9c755f', '#bab0ab'
    ];
    return palette[index % palette.length];
}

async function initCharts() {
    const data = await fetchData(`/Doctors/Faculty/getAvgGpa?facultyId=${FacId}`);
    if (!data) return;

    const departmentLabels = [];
    const datasetMap = {};
    const allGroupLabels = new Set();

    for (const dep of data) {
        departmentLabels.push(dep.departmentName);

        dep.groups.forEach(group => {
            const label = `${group.level} - ${group.gender}`;
            allGroupLabels.add(label);

            if (!datasetMap[label]) datasetMap[label] = [];
            datasetMap[label].push(group.avgGpa);
        });
    }

    allGroupLabels.forEach(label => {
        while (datasetMap[label].length < departmentLabels.length) {
            datasetMap[label].push(null);
        }
    });

    const datasets = [...allGroupLabels].map((label, index) => ({
        label,
        data: datasetMap[label],
        backgroundColor: getColor(index),
        borderWidth: 1
    }));

    new Chart(document.getElementById('GpaPerDepartment').getContext('2d'), {
        type: 'bar',
        data: {
            labels: departmentLabels,
            datasets
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Average GPA per Department (Grouped by Level & Gender)'
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4,
                    title: {
                        display: true,
                        text: 'GPA'
                    }
                }
            }
        }
    });
}

document.addEventListener('DOMContentLoaded', initCharts);




