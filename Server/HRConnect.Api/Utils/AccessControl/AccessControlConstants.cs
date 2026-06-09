namespace HRConnect.Api.Utils.AccessControl
{
  //This is used to migrate and backfill user to the new 
  //Role Based Access Control system
  public static class RoleSet
  {
    public const string NormalUser = "NormalUser";
    public const string SuperUser = "SuperUser";
    public const string Executive = "ExecutiveUser";
    public const string CEO = "CEO";
  }

  public static class PermissionSet
  {
    //Will Be Assigned To NormalUser
    public const string EmployeeViewOwn = "employee.view-own";
    public const string PayrollViewOwn = "payroll.view-own-payslips";
    public const string LeaveApply = "leave.apply";
    public const string LeaveViewOwn = "leave.view-own";
    public const string PayrollToolsCalculator = "payroll-tools.pension-calculator";
    //For Super User including the above
    public const string EmployeeViewAll = "employee.view-all";
    public const string EmployeeCreate = "employee.create";
    public const string EmployeeEdit = "employee.edit";
    public const string EmployeeViewPayslip = "employee.view-payslips-all";
    public const string CompanySwitch = "company.switch";
    public const string TaxManagePartial = "tax.manage-partial";
    public const string LeaveManagePartial = "leave.manage-partial";
    public const string PositionManagePartial = "position.manage-partial";
    public const string CompanyViewDetails = "company.view-details";
    //Additionally for the Executives (HOD)
    public const string BudgetSet = "budget.set";
    public const string BudgetView = "budget.view";
    public const string PayrollBenchmarkCapture = "payroll.benchmarking-capture";
    public const string PayrollBenchmarkView = "payroll.benchmarking-view";
    //Additionally for the CEO 
    public const string BudgetViewOnly = "budget.view-only";
    public const string BudgetApprove = "budget.approve";
    public const string BudgetComment = "budget.comment";
    public const string PayrollBenchmarkViewOnly = "payroll.benchmarking-view-only";
  }
}