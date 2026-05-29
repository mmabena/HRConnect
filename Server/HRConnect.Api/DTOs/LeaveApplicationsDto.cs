namespace HRConnect.Api.DTOs
{
  using System;
  public class CreateApplicationRequest
  {
    public string EmployeeId { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Description { get; set; }
  }
  public class LeaveApplicationResponse
  {
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DaysRequested { get; set; }
    public string Status { get; set; } = null!;
  }
  public class RejectLeaveRequest
  {
    public string Reason { get; set; } = string.Empty;
  }
}