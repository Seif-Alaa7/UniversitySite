const SubId = window.subjectId;

//fetching the data
async function fetchData() {
  try {
    const response = await fetch(`/Doctors/Subject/GetGrades?SubjectId=${SubId}`); 
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    console.log(data)
    return data
    
  } catch (error) {
    console.error('Error fetching data:', error);
  }
}
//toggle chart Start
function toggleFullWidth(chartElement) {
  const chartBox = chartElement.closest('.chartBox1, .chartBox2, .chartBox3, .chartBox4');
  if (chartBox) {
      chartBox.classList.toggle('full-width');
  }
}

document.querySelectorAll('canvas').forEach(canvas => {
  canvas.addEventListener('click', function() {
      toggleFullWidth(this);
  });
});
//Charting the data 
// setup 
const data = {
  labels: [],
  datasets: [{
    label: 'no. of students achive that grade ',
    data: [],
    backgroundColor: [
      'rgba(255, 26, 104, 0.2)',
      'rgba(54, 162, 235, 0.2)',
      'rgba(255, 26, 104, 0.2)',
      'rgba(54, 162, 235, 0.2)',
    ],
    borderColor: [
      'rgba(255, 26, 104, 1)',
      'rgba(54, 162, 235, 1)',
    ],
    borderWidth: 1
  }]
};

// config 
const config = {
  type: 'bar',
  data,
  options: {
    plugins: {
    legend: {
        display: false,
        
    }
},
      scales: {
          y: {
              beginAtZero: true,
              ticks: {
                  stepSize: 1, 
                  callback: function(value) {
                      if (Number.isInteger(value)) {
                          return value; 
                      }
                      return null; 
                  }
              }
          }
      }
  }
};


// render init block
const myChart = new Chart(
  document.getElementById('NoOfStudentsPerGrade'),
  config
);

//Charting the data 
// setup 
const dataHistogram = {
  labels: [],
  datasets: [{
    label: 'no. of students achive that degree ',
    data: [],
    backgroundColor: [
      'rgba(54, 162, 235, 0.8)'
    ],
    borderColor: [
      'rgba(0,0,0,1)'
    ],
    borderWidth: 1,
    barPercentage:1,
    categoryPercentage:1
  },

]
};

// config 
const configHistogram = {
  type: 'bar',
  data:dataHistogram,
  options: {
    plugins: {
      legend: {
          display: false,
          
      }
  },
      scales: {
          y: {
              beginAtZero: true,
              ticks: {
                  stepSize: 1, 
                  callback: function(value) {
                      if (Number.isInteger(value)) {
                          return value; 
                      }
                      return null; 
                  }
              }
          }
      }
  }
};


// render init block
const myChartHistogram = new Chart(
  document.getElementById('histogramChart'),
  configHistogram
);

//Fetching the data and updating the charts
fetchData().then(data => {
  const gradesArray = ['A+', 'A', 'B+', 'B', 'C+', 'C', 'D', 'F'];
  const scores=[] //I will push the degrees here
  // Count occurrences of each grade
  const gradeCount = {};
  data.forEach(index => {
      gradeCount[index.grade] = (gradeCount[index.grade] || 0) + 1;
  });
//Charting the data 
// setup 
const Top10data = {
  labels: [],
  datasets: [{
    label: 'Top 10 Students',
    data: [],
    backgroundColor: [
      'rgba(255, 26, 104, 0.2)',
      'rgba(54, 162, 235, 0.2)',
      'rgba(255, 26, 104, 0.2)',
      'rgba(54, 162, 235, 0.2)',
    ],
    borderColor: [
      'rgba(255, 26, 104, 1)',
      'rgba(54, 162, 235, 1)',
    ],
    borderWidth: 1
  }]
};

// config 
const configTop10 = {
  type: 'bar',
  data: Top10data,
  options: {
    indexAxis: 'y', // Horizontal bar chart
    plugins: {
      legend: {
        display: false,
      }
    },
    scales: {
      x: { // Keep 'x' for scores
        beginAtZero: true
      },
      y: { // Ensure student names appear
        ticks: {
          autoSkip: false, // Prevent skipping names
        }
      }
    }
  }
};


// render init block
const myChartTop10 = new Chart(
  document.getElementById('Top10'),
  configTop10
);//Charting the data 
// setup 
const Bottom10Data = {
  labels: [],
  datasets: [{
    label: 'Bottom10 Students',
    data: [],
    backgroundColor: [
      'rgba(255, 26, 104, 0.2)',
      'rgba(54, 162, 235, 0.2)',
      'rgba(255, 26, 104, 0.2)',
      'rgba(54, 162, 235, 0.2)',
    ],
    borderColor: [
      'rgba(255, 26, 104, 1)',
      'rgba(54, 162, 235, 1)',
    ],
    borderWidth: 1
  }]
};

// config 
const configBottom10 = {
  type: 'bar',
  data: Bottom10Data,
  options: {
    indexAxis: 'y', // Horizontal bar chart
    plugins: {
      legend: {
        display: false,
      }
    },
    scales: {
      x: { 
        beginAtZero: true
      },
      y: { 
        ticks: {
          autoSkip: false, // Prevent skipping names
        }
      }
    }
  }
};

// render init block
const myChartBottom10 = new Chart(
  document.getElementById('Bottom10'),
  configBottom10 
);
  
  const grades = [];
  const grades2 = [];
  gradesArray.forEach((grade, i) => {
      if (gradeCount[i] !== undefined) { 
          grades.push(grade); 
          grades2.push(gradeCount[i]); 
      }
  });
  //barchart for the grade End

  //histogram Start
  const degreeCounts = Array(10).fill(0); 
      data.forEach(item => {
        const degree = item.degree;
        const index = Math.floor(degree / 10); 
        if (index < 10) {
          degreeCounts[index]++;
        }
      });
//histogram End

//Top10 Students Start
data.sort((a, b) => b.degree - a.degree);
let Top10Names = data.slice(0,3).map(student => student.studentName);
let Top10Degrees = data.slice(0,3).map(student => student.degree);

//Top10 Students End

//bottom10 Students Start
let Bottom10Names = data.slice(-3).map(student => student.studentName);
let Bottom10Degrees = data.slice(-3).map(student => student.degree);




  //setting the data
  myChart.config.data.labels = grades;
  myChart.data.datasets[0].data = grades2;
  myChartHistogram.config.data.labels = ['0-9', '10-19', '20-29', '30-39', '40-49', '50-59', '60-69', '70-79', '80-89', '90-100'];
  myChartHistogram.data.datasets[0].data = degreeCounts;
  myChartTop10.config.data.labels =Top10Names
  myChartTop10.data.datasets[0].data =Top10Degrees
  myChartBottom10.config.data.labels =Bottom10Names
  myChartBottom10.data.datasets[0].data =Bottom10Degrees
  
  // Update the charts
  myChart.update();
  myChartHistogram.update();
  myChartTop10.update();
  myChartBottom10.update();
});

