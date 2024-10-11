document.addEventListener('DOMContentLoaded', function () {
    // دمج جميع الوظائف التي تعمل عند تحميل الصفحة
    const loader = document.querySelector('.loader-container');
    const header = document.querySelector('header');
    const main = document.querySelector('main');
    const footer = document.querySelector('footer');
    const navbar = document.querySelector('.navbar');
    const toggle = document.getElementById('theme-toggle');
    const body = document.body;
    const fadeElements = document.querySelectorAll('.fade-in');

    // إخفاء Loader وإظهار العناصر الأخرى
    setTimeout(function () {
        loader.style.display = 'none';
        header.style.display = 'block';
        main.style.display = 'block';
        footer.style.display = 'block';
    }, 3000);

    // التعامل مع التمرير
    function handleScroll() {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
        // التحقق من رؤية العناصر
        fadeElements.forEach(element => {
            const rect = element.getBoundingClientRect();
            if (rect.top < window.innerHeight && rect.bottom > 0) {
                element.classList.add('visible');
            }
        });
    }

    window.addEventListener('scroll', handleScroll);
    handleScroll();

    // التعامل مع تبديل الثيم
    if (localStorage.getItem('theme') === 'dark') {
        body.classList.add('dark-theme');
        toggle.classList.replace('fa-sun', 'fa-moon');
    }

    toggle.addEventListener('click', () => {
        body.classList.toggle('dark-theme');
        if (body.classList.contains('dark-theme')) {
            localStorage.setItem('theme', 'dark');
            toggle.classList.replace('fa-sun', 'fa-moon');
        } else {
            localStorage.setItem('theme', 'light');
            toggle.classList.replace('fa-moon', 'fa-sun');
        }
    });
    // تحميل الصورة
    var img = new Image();
    img.src = '/img9.png';
    img.onload = function () {
        document.querySelector('.header').classList.add('loaded');
    };

    // إضافة مستمعي الأحداث لأزرار الحذف
    ['delete-forever', 'delete-from-department', 'delete-button'].forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('click', handleDelete);
        }
    });
});

function handleDelete(event, customMessage) {
    event.preventDefault();

    const element = event.currentTarget; 
    const url = element.href; 

    const isArabic = element.id === 'delete-button'; 

    Swal.fire({
        title: isArabic ? 'هل أنت متأكد؟' : 'Are you sure?',
        text: customMessage,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: isArabic ? 'نعم، احذفها!' : 'Yes, delete it!',
        cancelButtonText: isArabic ? 'إلغاء' : 'Cancel',
        customClass: {
            title: 'text-xl font-bold',
            confirmButton: 'bg-red-600 text-white font-bold px-4 py-2 rounded',
            cancelButton: 'bg-gray-400 text-white font-bold px-4 py-2 rounded'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            if (element.id === 'delete-department') {
                window.location.href = url;
            } else {
                window.location.href = element.href;
            }
        }
    });
}
function confirmDelete() {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            var form = document.getElementById('deleteForm');
            form.submit();
        }
    });
}
