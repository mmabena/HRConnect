import "../MenuBar.css";
import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";

const MenuBar = ({ currentUser, onAccessDenied }) => {
  const [reportOpen, setReportOpen] = useState(false);
  const [companyOpen, setCompanyOpen] = useState(false);
  const [adminOpen, setAdminOpen] = useState(false);
  const [manualReportToggle, setManualReportToggle] = useState(false);
  const [manualAdminToggle, setManualAdminToggle] = useState(false);

  const navigate = useNavigate();
  const location = useLocation();

  // FIX: Access the role directly from the currentUser object
  const role = currentUser?.role?.toLowerCase() || "";
  console.log("Role", role);
  const isAdminOrSuperUser = role === "admin" || role === "superuser";

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
                src="/images/contacts_product.png"
                alt="Personal icon"
                className="menu-icon"
              />
              <span className="menu-heading">Personal</span>
            </div>
          </li>

          {/* Employee Management */}
          {isAdminOrSuperUser && (
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
                      onClick={() => handleSubmenuClick("/addEmployee")}
                    >
                      Add Employee
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
                </ul>
              )}
            </li>
          )}

          {/* ✅ Company Management */}
          {isAdminOrSuperUser && (
            <li>
              <div className="menu-item-wrapper" onClick={toggleCompany}>
                <img
                  src="/images/autostop.png"
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
                      onClick={() => handleSubmenuClick("/addCompany")}
                    >
                      Add Company
                    </span>
                  </li>
              
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => handleSubmenuClick("/companyManagement")}
                    >
                      Company List
                    </span>
                  </li>
                        <li>
                    <span
                      className="menu-subitem"
                      style={{ cursor: "pointer" }}
                      onClick={() => navigate("/company-contribution")}
                    >
                      Company Contribution
                    </span>
                  </li>
                </ul>
              )}
            </li>
          )}

          {/* Payroll Management */}
          {isAdminOrSuperUser && (
             <li>
            <div 
              className="menu-item-wrapper"
              onClick={() => navigate("/compensationPlanning")} // <-- Add this onClick handler
            >
              <img
                src="/images/regular_expression.png"
                alt="Payroll icon"
                className="menu-icon"
              />
              <span className="menu-heading">Compensation Planning</span>
            </div>
          </li>
          )}

          {/* Document Management */}
          {isAdminOrSuperUser && (
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
          {role === "superuser" && (
            <li>
              <div className="menu-item-wrapper" onClick={toggleAdmin}>
                <img
                  src="/images/attach_file.png"
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
                      User Management
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
          {/* Initials Circle */}
          <div className="menu-initials-circle">
            {(
              (currentUser?.firstName || "").charAt(0) +
              (currentUser?.lastName || "").charAt(0)
            ).toUpperCase()}
          </div>
          <div className="user-text-details">
            <div className="user-full-name">
              {currentUser?.firstName} {currentUser?.lastName}
            </div>
            <div className="user-job-title">
              {/* FIX: Access the jobTitle directly from the currentUser object */}
              {currentUser?.jobTitle}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MenuBar;