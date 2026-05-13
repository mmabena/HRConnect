import "./MenuBar.css";
import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import api from "../../../src/api/api.js";
import { fetchNotifications } from "../../Pages/NotificationPage/notificationsApi.js";

const MenuBar = ({ currentUser, onAccessDenied, onLogout }) => {
  const [showOptions, setShowOptions] = useState(false);
  const [activeIndex, setActiveIndex] = useState(null);
  const [activeMenu, setActiveMenu] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [bellCount, setBellCount] = useState(0);
  const [deductionsOpen, setDeductionsOpen] = useState(false);
  const [openSubmenu, setOpenSubmenu] = useState(null);
  // FIX: Access the role directly from the currentUser object
  const role = currentUser?.role?.toLowerCase();

  const displayName = currentUser?.username || currentUser?.email || "User";
  const [canProjectPension, setCanProjectPension] = useState(false);

  //displaying user initials
  const initials = displayName
    .split(" ")
    .map((name) => name.charAt(0))
    .join("")
    .substring(0, 2)
    .toUpperCase();

  const navigate = useNavigate();
  const location = useLocation();

  const isActive = (paths) => {
    return paths.some((path) => location.pathname.startsWith(path));
  };

  const handleHeadingClick = (index, toggleFunction) => {
    setActiveIndex((prev) => (prev === index ? null : index));
    toggleFunction();
  };

  const permissions = {
    isAdmin: ["admin", "superuser"].includes(role),
    isNormalUser: role === "normaluser",
  };

  const isEmployeeManagementPage =
    location.pathname.startsWith("/employeeList") ||
    location.pathname.startsWith("/addEmployee") ||
    location.pathname.startsWith("/employeeList") ||
    location.pathname.startsWith("/editEmployee");

  const isUserManagementPage = location.pathname.startsWith("/userManagement");

  const baseUrl = process.env.REACT_APP_API_BASE_URL;

  // This loads all notifications from the database
  useEffect(() => {
    let cancelled = false;

    const loadNotifications = async () => {
      try {
        if (!role) return;

        const data = await fetchNotifications(role);

        if (!cancelled) {
          setNotifications(data);
          setBellCount(data.filter((n) => !n.read).length);
        }
      } catch (err) {
        console.error("Failed to load notifications:", err);
        if (!cancelled) setBellCount(0);
      }
    };

    loadNotifications();

    return () => {
      cancelled = true;
    };
  }, [role]);

  useEffect(() => {
    console.log("MenuBar user role:", role);
  }, [currentUser, role]);

  useEffect(() => {
    if (!role) return;
    if (isEmployeeManagementPage) setActiveMenu("report");
    else if (isUserManagementPage) setActiveMenu("admin");
  }, [role, location.pathname]);

  useEffect(() => {
    if (
      localStorage.getItem("currentUser") !== null &&
      localStorage.getItem("currentUser") !== undefined
    ) {
      const token = localStorage.getItem("token");
      const email = JSON.parse(localStorage.getItem("currentUser")).email;
      const decodedTokenEmail = jwtDecode(token).sub;
      if (decodedTokenEmail == email) {
        try {
          api
            .get(`${baseUrl}/employee/email/${email}`, {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            })
            .then((response) => {
              if (response.status === 200) {
                const employementStatus = response.data.employmentStatus;
                const employeeAge = response.data.dateOfBirth;

                if (
                  employementStatus === "Permanent" &&
                  calculateAge(employeeAge) < 65
                ) {
                  setCanProjectPension(true);
                  console.log("Employee date of birth:", employeeAge);
                  console.log("Employment status:", employementStatus);
                }
              } else {
                console.error("Unexpeted status:", response.status);
              }
            })
            .then((response) => {
              if (response.status === 200) {
                const employementStatus = response.data.employmentStatus;
                const employeeAge = response.data.dateOfBirth;
                if (employementStatus === 0 && calculateAge(employeeAge) < 65) {
                  setCanProjectPension(true);
                }
              } else {
                console.error("Unexpeted status:", response.status);
              }
            })
            .catch((error) => {
              console.error("Error:", error);
            });
        } catch (error) {
          console.error("Failed to fetch your employee details:", error);
        }
      } else {
        console.error("User data may have changed without authorization");
      }
    }
  }, []);

  const calculateAge = (dateOfBirth) => {
    let today = new Date();
    let birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();

    if (today.getMonth() < birthDate.getMonth()) {
      age--;
    } else if (
      today.getMonth() === birthDate.getMonth() &&
      today.getDay() < birthDate.getDay()
    ) {
      age--;
    }

    return age;
  };

  useEffect(() => {
    const handleClickOutside = () => {
      setShowOptions(false);
    };

    if (showOptions) {
      document.removeEventListener("click", handleClickOutside);
    }

    return () => {
      document.removeEventListener("click", handleClickOutside);
    };
  }, [showOptions]);

  const handleSubmenuClick = (path) => {
    navigate(path);
    onAccessDenied && onAccessDenied("");
  };

  const toggleOptions = (e) => {
    e.stopPropagation();
    setShowOptions((prev) => !prev);
  };

  function toggleMenu(menuName) {
    setActiveMenu((prev) => (prev === menuName ? null : menuName));
  }

  const menuPaths = {
    0: ["/personal"],
    1: [
      "/employeeList",
      "/terminateemployee",
      "/transferemployee",
      "/trnsferhistory",
    ],
    2: [
      "/taxTableManagement",
      "/leavemanagement",
      "/positionManagement",
      "/company-details",
      "/salarybudgets",
    ],
    3: [
      "/earnings",
      "/pension-funds",
      "/assign-pension",
      "/medical-aid",
      "/company-contributions",
      "/bcea",
      "/oid",
      "/stock",
    ],
    4: ["/userManagement"],
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
              <span
                className="menu-heading"
                onClick={() => handleSubmenuClick("/personal")}
              >
                Personal Information
              </span>
            </div>
          </li>
          {/* Employee Management */}
          {permissions.isAdmin && (
            <li>
              <div
                className="menu-item-wrapper"
                onClick={() => toggleMenu("report")}
              >
                <img
                  src="/images/cases.png"
                  alt="Employee Management"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Employee Management
                  <span className="menu-dropdown">
                    {activeMenu === "report" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {activeMenu === "report" && (
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
              <div
                className="menu-item-wrapper"
                onClick={() => toggleMenu("company")}
              >
                <img
                  src="/images/building-2.png"
                  alt="Company Management"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Company Management
                  <span className="menu-dropdown">
                    {activeMenu === "company" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {activeMenu === "company" && (
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
                onClick={() => toggleMenu("pay")}
              >
                <img
                  src="/images/hand-coins.png"
                  alt="Payroll icon"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Payroll Management
                  <span className="menu-dropdown">
                    {activeMenu === "pay" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {activeMenu === "pay" && (
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
                    <div
                      className="menu-item-wrapper"
                      onClick={(e) => {
                        e.stopPropagation();
                        setOpenSubmenu((prev) =>
                          prev === "deductions" ? null : "deductions",
                        );
                      }}
                    >
                      <span>Deductions</span>
                      <span className="menu-dropdown">
                        {openSubmenu === "deductions" ? "▲" : "▼"}
                      </span>
                    </div>
                    {openSubmenu === "deductions" && (
                      <ul className="submenu show">
                        <li>
                          <span
                            className="menu-subitem"
                            onClick={() => handleSubmenuClick("/pension-funds")}
                          >
                            Pension Funds
                          </span>
                        </li>
                        <li>
                          <span
                            className="menu-subitem"
                            onClick={() =>
                              handleSubmenuClick("/assign-pension")
                            }
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
                      onClick={() =>
                        handleSubmenuClick("/company-contributions")
                      }
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
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() => navigate("/salarybenchmark")}
                    >
                      Salary Benchmark
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
              <div
                className="menu-item-wrapper"
                onClick={() => toggleMenu("admin")}
              >
                <img
                  src="/images/user-star.png"
                  alt="Admin Tools icon"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Admin Management tools
                  <span className="menu-dropdown">
                    {activeMenu === "admin" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {activeMenu === "admin" && (
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
              <div
                className="menu-item-wrapper"
                onClick={() => toggleMenu("payrollInfo")}
              >
                <img
                  src="/images/hand-coins.png"
                  alt="Leave"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Payroll Information
                  <span className="menu-dropdown">
                    {activeMenu === "payrollInfo" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {activeMenu === "payrollInfo" && (
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
              <div
                className="menu-item-wrapper"
                onClick={() => toggleMenu("leave")}
              >
                <img
                  src="/images/file-user.png"
                  alt="Leave"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Leave
                  <span className="menu-dropdown">
                    {activeMenu === "leave" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {activeMenu === "leave" && (
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
              <div
                className="menu-item-wrapper"
                onClick={() => toggleMenu("payroll")}
              >
                <img
                  src="/images/calculator.png"
                  alt="Payroll Tools"
                  className="menu-icon"
                />
                <span className="menu-heading">
                  Payroll Tools
                  <span className="menu-dropdown">
                    {activeMenu === "payroll" ? "▲" : "▼"}
                  </span>
                </span>
              </div>
              {canProjectPension && activeMenu === "payroll" && (
                <ul className="submenu show">
                  <li>
                    <span
                      className="menu-subitem"
                      onClick={() =>
                        handleSubmenuClick("/projection-calculator")
                      }
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
        {/* Container for user details */}
        <div className="user-details-container">
          <div className="menu-initials-circle" onClick={toggleOptions}>
            {initials}
            {showOptions && (
              <div className="user-dropdown">
                <button
                  className="dropdown-item"
                  onClick={() => {
                    setShowOptions(false);
                    navigate("/changePassword");
                  }}
                >
                  Change Password
                </button>

                <button
                  className="dropdown-item logout"
                  onClick={() => {
                    setShowOptions(false);
                    onLogout();
                  }}
                >
                  Logout
                </button>
              </div>
            )}
          </div>
          <div className="user-text-details">
            <div className="user-full-name">{displayName}</div>
            <div className="user-job-title">{currentUser?.jobTitle}</div>
          </div>

          <div className="menu-icon-wrapper">
            <div className="menu-icons-wrapper">
              <img
                src="/images/bell.svg"
                alt="Bell icon"
                className="menu-icon"
                onClick={() => {
                  navigate("/notifications", { state: { role: role } });
                }}
              />

              {/* Dynamic unread badge */}
              {bellCount > 0 && (
                <span
                  className="notification-badge"
                  data-count={bellCount > 99 ? "99+" : bellCount}
                >
                  {bellCount > 99 ? "99+" : bellCount}
                </span>
              )}
            </div>

            <img
              src="/images/setitngs_icon.png"
              alt="Settings icon"
              className="menu-icon"
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default MenuBar;
