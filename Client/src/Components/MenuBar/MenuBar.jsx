import "./MenuBar.css";
import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";

const MenuBar = ({ currentUser, onAccessDenied, onLogout }) => {
  const [reportOpen, setReportOpen] = useState(false);
  const [companyOpen, setCompanyOpen] = useState(false);
  const [adminOpen, setAdminOpen] = useState(false);
  const [deductionsOpen, setDeductionsOpen] = useState(false);
  const [payrollOpen, setPayrollOpen] = useState(false);
  const [leaveOpen, setLeaveOpen] = useState(false);
  const [payOpen, setPayOpen] = useState(false);
  const [payinfoOpen, setPayInfoOpen] = useState(false);
  const [manualReportToggle, setManualReportToggle] = useState(false);
  const [manualAdminToggle, setManualAdminToggle] = useState(false);
  const [showMenu, setShowMenu] = useState(false);

  //displaying user initials
  const displayName =
  currentUser?.username ||
  currentUser?.email ||
  "User";

  const initials = displayName
    .split(" ")
    .map(name => name.charAt(0))
    .join("")
    .substring(0, 2)
    .toUpperCase();
  
  const navigate = useNavigate();
  const location = useLocation();

  // FIX: Access the role directly from the currentUser object
  const role = currentUser?.role?.toLowerCase();

  const permissions = {
    isAdmin: ["admin", "superuser"].includes(role),
    isNormalUser: role === "normaluser"
  };

  const isEmployeeManagementPage =
    location.pathname.startsWith("/addEmployee") ||
    location.pathname.startsWith("/employeeList") ||
    location.pathname.startsWith("/editEmployee");

  const isUserManagementPage = location.pathname.startsWith("/userManagement");

  useEffect(() => {
    console.log("MenuBar user role:", role);
  }, [currentUser, role]);

  useEffect(() => {
    if (!role) return;

    if (isEmployeeManagementPage && !manualReportToggle) {
      setReportOpen(true);
    } else if (!manualReportToggle) {
      setReportOpen(false);
    }

    if (isUserManagementPage && !manualAdminToggle) {
      setAdminOpen(true);
    } else if (!manualAdminToggle) {
      setAdminOpen(false);
    }
  }, [
    role,
    location.pathname,
    manualReportToggle,
    manualAdminToggle,
    isEmployeeManagementPage,
    isUserManagementPage,
  ]);

  useEffect(() => {
    const handleClickOutside = () => {
      setShowMenu(false);
    };

    if (showMenu) {
      document.removeEventListener("click", handleClickOutside);
    }
     
    return () => {
      document.removeEventListener("click", handleClickOutside);
    };
  }, [showMenu]);

  const toggleMenu = () => {
    setShowMenu(prev => !prev);
  };

  const toggleReport = () => {
    setManualReportToggle(true);
    setReportOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  };

  const toggleAdmin = () => {
    setManualAdminToggle(true);
    setAdminOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  };

  const toggleCompany = () => {
    setCompanyOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  };

  const toggleDeductions = (e) => {
    e.stopPropagation(); 
    setDeductionsOpen((prev) => !prev);
  };

  const togglePay = () => {
    setPayOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  };

  const togglePayroll = () => {
    setPayrollOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  }

  const toggleLeave = () => {
    setLeaveOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  }

  const togglePayrollInfo = () => {
    setPayInfoOpen((prev) => !prev);
    onAccessDenied && onAccessDenied("");
  }

  const handleSubmenuClick = (path) => {
    navigate(path);
    onAccessDenied && onAccessDenied("");
  };

  return (
    <div className="menu-bar-container">
      <div className="menu-inner">
        <div className="menu-logo-wrapper">
          <span className="menu-bar-logo-text-bold">singular</span>
          <span className="menu-bar-logo-text-light">express</span>
        </div>

        <ul className="menu-list">
          {/* ✅ Personal - Static, no toggle */}
          <li>
            <div className="menu-item-wrapper">
              <img
                src="/images/user.png"
                alt="Personal icon"
                className="menu-icon"
              />
              <span className="menu-heading">Personal Information</span>
            </div>
          </li>

          {/* Employee Management */}
          {permissions.isAdmin && (
            <li>
              <div className="menu-item-wrapper" onClick={toggleReport}>
                <img
                  src="/images/cases.png"
                  alt="Employee Management"
                  className="menu-icon"
                                                 />
                <span className="menu-heading">
                  Employee Management
                  <span className="menu-dropdown">{reportOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {reportOpen && (
                <ul className="submenu show">
                   <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/employeeList")}
                    >
                      Employee List
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/employeeList")}
                    >
                      Employee List
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/addEmployee")}
                    >
                      Add New Employee
                    </span>
                  </li>
                   <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/editEmployee")}
                    >
                      Edit Employee
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/terminateemployee")}
                    >
                      Terminate Employee
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/transferemployee")}
                    >
                      Transfer Employee
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/trnsferhistory")}
                    >
                      Transfer History
                    </span>
                  </li>
                </ul>
              )}
            </li>
          )}

          {/* ✅ Company Management */}
          {permissions.isAdmin && (
            <li>
              <div className="menu-item-wrapper" onClick={toggleCompany}>
                <img
                  src="/images/building-2.png"
                  alt="Company Management"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Company Management
                  <span className="menu-dropdown">{companyOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {companyOpen && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/taxtablemanagement")}
                    >
                      Tax Table Management
                    </span>
                  </li>
              
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/leavemanagement")}
                    >
                      Leave Management
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => navigate("/positionManagement")}
                    >
                      Position Management
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => navigate("/company-contribution")}
                    >
                      Company Details
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => navigate("/salarybudgets")}
                    >
                      Salary Budgets
                    </span>
                  </li>
                </ul>
              )}
            </li>
          )}

          {/* Payroll Management */}
          {permissions.isAdmin && (
            <li>
              <div
                className="menu-item-wrapper"
                onClick={togglePay} // <-- Add this onClick handler
              >
                <img
                  src="/images/hand-coins.png"
                  alt="Payroll icon"
                  className="menu-icon"
                />
                <span className="menu-heading">Payroll Management
                  <span className="menu-dropdown">{payOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {payOpen && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/earnings")}
                    >
                      Earnings
                    </span>
                  </li>
              
                  <li>
                    <div className="menu-item-wrapper" onClick={toggleDeductions}>
                      <span>Deductions</span>
                      <span className="menu-dropdown">{deductionsOpen ? "▲" : "▼"}</span>
                    </div>
                    {deductionsOpen && (
                      <ul className="submenu show">
                        <li>
                          <span className="menu-subitem" onClick={() => handleSubmenuClick("/pension-funds")}>
                            Pension Funds
                          </span>
                        </li>
                        <li>
                          <span
                            className="menu-subitem"
                            onClick={() => handleSubmenuClick("/assign-pension")}
                          >
                            Assign Pension
                          </span>
                        </li>
                        <li>
                          <span
                            className="menu-subitem"
                            onClick={() => handleSubmenuClick("/medical-aid")}
                          >
                            Medical Aid
                          </span>
                        </li>
                      </ul>
                    )}
                  </li>
                                    
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/company-contributions")}
                    >
                      Company Contributions
                    
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/bcea")}
                    >
                      BCEA
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/oid")}
                    >
                      OID
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/stock")}
                    >
                      Stock
                    </span>
                  
                  </li>
                </ul>
              )}
            </li>
          )}
          
          {/* Document Management */}
          {permissions.isAdmin && (
            <li>
              <div className="menu-item-wrapper">
                <img
                  src="/images/savings.png"
                  alt="Document icon"
                  className="menu-icon"
                />
                <span className="menu-heading">Document Management</span>
              </div>
            </li>
          )}

          {/* Admin tools (SuperUser only) */}
          {permissions.isAdmin && (
            <li>
              <div className="menu-item-wrapper" onClick={toggleAdmin}>
                <img
                  src="/images/user-star.png"
                  alt="Admin Tools icon"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Admin Management tools
                  <span className="menu-dropdown">{adminOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {adminOpen && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/userManagement")}
                    >
                      Roles
                    </span>
                  </li>
                </ul>
              )}
            </li>
          )}

          {/* NormalUser tools (NormalUser only) */}
          {permissions.isNormalUser && (
            <li>
              <div className="menu-item-wrapper" onClick={togglePayrollInfo}>
                <img
                  src="/images/hand-coins.png"
                  alt="Leave"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Payroll Information
                  <span className="menu-dropdown">{payinfoOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {payinfoOpen && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/payslips")}
                    >
                      Payslips
                    </span>
                  </li>
                </ul>
              )}
            </li>
          )}

          {/* NormalUser tools (NormalUser only) */}
          {permissions.isNormalUser && (
            <li>
              <div className="menu-item-wrapper" onClick={toggleLeave}>
                <img
                  src="/images/file-user.png"
                  alt="Leave"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Leave
                  <span className="menu-dropdown">{leaveOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {leaveOpen && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/leave-application")}
                    >
                      Leave Application
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/leave-balance")}
                    >
                      Leave Balance
                    </span>
                  </li>
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/history")}
                    >
                      History
                    </span>
                  </li>
                </ul>
              )}
            </li>
          )}

          {/* NormalUser tools (NormalUser only) */}
          {permissions.isNormalUser && (
            <li>
              <div className="menu-item-wrapper" onClick={togglePayroll}>
                <img
                  src="/images/calculator.png"
                  alt="Payroll Tools"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Payroll Tools
                  <span className="menu-dropdown">{payrollOpen ? "▲" : "▼"}</span>
                </span>
              </div>
              {payrollOpen && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/projection-calculator")}
                    >
                      Projection Calculator
                    </span>
                  </li>
                </ul>
              )}
            </li> 
          )}
        </ul>
                 
      </div>
      
      <div className="menu-footer">
        <img
          src="/images/setitngs_icon.png"
          alt="Settings icon"
          className="menu-icon"
        />
        {/* Container for user details */}
        <div className="user-details-container">
          <div className="menu-initials-circle" onClick={(e) => {
            e.stopPropagation();
            toggleMenu();
          }}>
            
            {initials}
            {showMenu && (
                <div className="user-dropdown">
                  <button
                    className="dropdown-item"
                    onClick={() => {
                      setShowMenu(false);
                      navigate("/changePassword");
                    }}
                  >
                    Change Password
                  </button>

                  <button
                    className="dropdown-item logout"
                    onClick={() => {
                      setShowMenu(false);
                      onLogout();
                    }}
                  >
                    Logout
                  </button>
                </div>
              )}
          </div>
          <div className="user-text-details">
            <div className="user-full-name">
              {displayName}
            </div>
            <div className="user-job-title">
              {currentUser?.jobTitle}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MenuBar;