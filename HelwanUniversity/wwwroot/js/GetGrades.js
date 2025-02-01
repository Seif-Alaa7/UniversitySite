//fetching the data
async function fetchData() {
  try {
    const response = await fetch('/Doctors/Subject/GetGrades?SubjectId=12'); // Replace with your actual API URL
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

  // Create unique labels and corresponding counts
  const grades = [];
  const grades2 = [];

  gradesArray.forEach((grade, i) => {
      if (gradeCount[i] !== undefined) { // If the grade index exists in data
          grades.push(grade); // Push the grade label
          grades2.push(gradeCount[i]); // Push the count of students with that grade
      }
  });
  const degreeCounts = Array(10).fill(0); // For degree intervals 0-9, 10-19, ..., 90-100
      data.forEach(item => {
        const degree = item.degree;
        const index = Math.floor(degree / 10); // Calculate interval
        if (index < 10) {
          degreeCounts[index]++;
        }
      });

  //setting the data
  myChart.config.data.labels = grades;
  myChart.data.datasets[0].data = grades2;
  myChartHistogram.config.data.labels = ['0-9', '10-19', '20-29', '30-39', '40-49', '50-59', '60-69', '70-79', '80-89', '90-100'];
  myChartHistogram.data.datasets[0].data = degreeCounts;
  
  // Update the charts
  myChart.update();
  myChartHistogram.update();

});
