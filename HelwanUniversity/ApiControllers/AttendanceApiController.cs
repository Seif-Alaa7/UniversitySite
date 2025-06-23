
using Microsoft.AspNetCore.Mvc;
using Data;
using Models;
using ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Data.Repository.IRepository;

[ApiController]
[Route("api/[controller]")]
public class AttendanceApiController : ControllerBase
{
    private readonly IAttendanceRecordRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AttendanceApiController(
        IAttendanceRecordRepository attendanceRepository,
        IStudentRepository studentRepository, 
        ISubjectRepository subjectRepository,
        IDoctorRepository doctorRepository)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _subjectRepository = subjectRepository;
        _doctorRepository = doctorRepository;
    }

    [HttpPost("Record")]
    public async Task<IActionResult> RecordAttendance([FromBody] AttendanceApiRequestVM request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
        if (subject == null)
        {
            return NotFound($"Subject with ID {request.SubjectId} not found.");
        }
        int? doctorIdToUse = request.DoctorId;

        if (!request.DoctorId.HasValue)
        {
            doctorIdToUse = subject.DoctorId;
        }
        foreach (var attendee in request.Attendees)
        {
            var student = await _studentRepository.GetByIdAsync(attendee.StudentId);
            if (student == null)
            {
                return NotFound($"Student with ID {attendee.StudentId} not found.");
            }

            bool hasAttended = await _attendanceRepository.HasStudentAttendedSubjectOnDateAsync(
                attendee.StudentId, request.SubjectId, request.AttendanceDate.Date);

            if (!hasAttended)
            {
                var attendanceRecord = new AttendanceRecord
                {
                    StudentId = attendee.StudentId,
                    SubjectId = request.SubjectId,
                    AttendanceDate = request.AttendanceDate.Date,
                    DoctorId = doctorIdToUse,
                    AiSessionId = request.AiSessionId
                };
                await _attendanceRepository.AddAsync(attendanceRecord);
            }
        }

        await _attendanceRepository.SaveAsync();

        return Ok("Attendance records processed successfully.");
    }
    [HttpGet("StudentDetails/{studentId}")]
    public async Task<IActionResult> GetStudentDetails(int studentId)
    {
        if (studentId <= 0)
        {
            return BadRequest("Student ID must be a positive integer.");
        }

        var student = await _studentRepository.GetByIdAsync(studentId);

        if (student == null)
        {
            return NotFound($"Student with ID {studentId} not found in the database.");
        }

        var studentDetails = new Student
        {
            Id = student.Id,
            Name = student.Name
        };

        return Ok(studentDetails);
    }
}