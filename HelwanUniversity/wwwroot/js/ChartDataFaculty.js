let currentState = {
    facultyId: window.FacultyId,
    departmentId: null,
    level: null
};

let departmentMap = {};

async function fetchData(url) {
    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
        return await res.json();
    } catch (err) {
        console.error('❌ Error fetching data:', err);
        return [];
    }
}

async function loadDepartments() {
    const data = await fetchData(`/Doctors/Faculty/GetDepartments?facultyId=${currentState.facultyId}`);
    data.forEach(dep => {
        departmentMap[dep.name] = dep.id;
    });
}

function findDepartmentIdByName(name) {
    return departmentMap[name] || null;
}

async function initChart() {
    if (!currentState.departmentId) {
        const data = await fetchData(`/Doctors/Faculty/GetAvgGpaByDepartment?facultyId=${currentState.facultyId}`);
        renderChart(data, 'departmentName', 'avgGpa', 'Departments', onDepartmentClick);
    } else if (!currentState.level) {
        const data = await fetchData(`/Doctors/Faculty/GetAvgGpaByLevel?departmentId=${currentState.departmentId}`);
        renderChart(data, 'level', 'avgGpa', 'Levels', onLevelClick);
    } else {
        const data = await fetchData(`/Doctors/Faculty/GetAvgGpaByGender?departmentId=${currentState.departmentId}&level=${currentState.level}`);
        renderChart(data, 'gender', 'avgGpa', 'Genders', null);
    }
}

function renderChart(data, labelKey, valueKey, title, clickHandler) {
    const labels = data.map(d => d[labelKey]);
    const values = data.map(d => d[valueKey]);

    let backgroundColors;

    if (title === "Genders") {
        backgroundColors = labels.map(label => {
            if (label === "Male") {
                return 'rgba(74, 144, 226, 0.6)'; // Blue
            } else if (label === "Female") {
                return 'rgba(255, 105, 180, 0.6)'; // Pink
            }
            return 'rgba(180, 180, 180, 0.6)'; // Gray fallback
        });
    } else {
        backgroundColors = labels.map(() => 'rgba(54, 162, 235, 0.6)');
    }

    const ctx = document.getElementById('GpaPerDepartment').getContext('2d');
    if (window.chartInstance) window.chartInstance.destroy();

    window.chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: title,
                data: values,
                backgroundColor: backgroundColors
            }]
        },
        options: {
            onClick: (evt, elements) => {
                if (!elements.length || !clickHandler) return;
                const index = elements[0].index;
                clickHandler(labels[index]);
            },
            plugins: {
                title: {
                    display: true,
                    text: `Average GPA - ${title}`
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4
                }
            }
        }
    });

    renderBackButton();
}



function onDepartmentClick(departmentName) {
    currentState.departmentId = findDepartmentIdByName(departmentName);
    initChart();
}

function onLevelClick(level) {
    currentState.level = level;
    initChart();
}

function renderBackButton() {
    const btn = document.getElementById('backBtn');
    if (!btn) return;

    btn.style.display = currentState.departmentId ? 'block' : 'none';

    btn.onclick = () => {
        if (currentState.level) {
            currentState.level = null;
        } else if (currentState.departmentId) {
            currentState.departmentId = null;
        }
        initChart();
    };
}

document.addEventListener('DOMContentLoaded', async () => {
    await loadDepartments();
    initChart();
});

function toggleFullWidthChart() {
    const box = document.querySelector('.chartBox4');
    box.classList.toggle('full-width');
}

function goBack() {
    if (currentState.level) {
        currentState.level = null;
    } else if (currentState.departmentId) {
        currentState.departmentId = null;
    }
    initChart();
}






