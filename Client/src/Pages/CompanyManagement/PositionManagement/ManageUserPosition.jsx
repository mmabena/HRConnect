import React, { useState, useEffect } from "react";
import CompanyManagementHeader from "../../../Components/CompanyManagement/companyManagementHeader";
import CompanyManagementNavBar from "../../../Components/CompanyManagement/companyManagementNavBar";
import { useNavigate } from "react-router-dom";
import api from "../../../api/api";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const ManageUserPositions = ({ title }) => {
  const [employees, setEmployees] = useState([]);
  const [positions, setPositions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);
  const [currentTime, setCurrentTime] = useState('');
    const [currentDate, setCurrentDate] = useState('');

  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);
  const [showPageOptions, setShowPageOptions] = useState(false);
  const [changes, setChanges] = useState({});

  const [activeTab, setActiveTab] = useState("Position Management");

  const navigate = useNavigate();
  const pageOptions = [10, 15, 20, 25];

  const navTabs = [
    "Tax Table Management",
    "Company Details",
    "Leave Management",
    "Position Management",
    "Manage Companies",
    "Salary Budgets",
  ];

  const tabWidths = [168, 133, 122, 134, 154, 125];
  useEffect(() => {
    const updateDateTime = () => {
      const now = new Date();

      const month = now.toLocaleDateString('en-ZA', { month: 'short' });
      const day = now.toLocaleDateString('en-ZA', { day: '2-digit' });
      const year = now.toLocaleDateString('en-ZA', { year: 'numeric' });

      const time = now.toLocaleTimeString('en-ZA', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      });

      setCurrentDate(`${month}. ${day}, ${year}`);
      setCurrentTime(time);
    };

    updateDateTime();
    const intervalId = setInterval(updateDateTime, 60000);
    return () => clearInterval(intervalId);
  }, []);

  // ==============================
  // INITIALIZATION + AUTH
  // ==============================
  useEffect(() => {
    const initialize = async () => {
      setLoading(true);
      const token = localStorage.getItem("token");

      if (!token) {
        toast.error("You are not logged in.");
        setLoading(false);
        return;
      }

      try {
        const decoded = jwtDecode(token);
        const role =
          decoded?.role ||
          decoded?.[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ];

        if (role !== "SuperUser") {
          toast.error("Access Denied. SuperUser only.");
          setHasAccess(false);
          setLoading(false);
          return;
        }

        setHasAccess(true);

        // Fetch employees + positions
        const [employeesRes, positionsRes] = await Promise.all([
          api.get("/employee"),
          api.get("/positions"),
        ]);

        setEmployees(employeesRes.data);
        setPositions(positionsRes.data);
      } catch (error) {
        console.error("Initialization error:", error);
        toast.error("Failed to load data.");
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  // ==============================
  // POSITION LOOKUP MAP
  // ==============================
  const positionMap = Object.fromEntries(
    positions.map((pos) => [pos.positionId, pos.positionTitle])
  );

  // ==============================
  // PAGINATION
  // ==============================
  const totalPages = Math.ceil(employees.length / itemsPerPage);
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentEmployees = employees.slice(
    indexOfFirstItem,
    indexOfLastItem
  );

  const handlePrev = () =>
    currentPage > 1 && setCurrentPage((prev) => prev - 1);

  const handleNext = () =>
    currentPage < totalPages && setCurrentPage((prev) => prev + 1);

  const handlePageClick = (num) => setCurrentPage(num);
  // ==============================
  // SAVE CHANGES
  // ==============================
  const handleSave = async () => {
    const updates = Object.entries(changes).map(([employeeId, positionId]) => ({
      employeeId,
      positionId,
    }));

    if (updates.length === 0) {
      toast.info("No changes to save.");
      return;
    }

    try {
      await api.put("/employee/updatePositions", { updates });
      toast.success("Positions updated successfully.");
      setChanges({});
    } catch (error) {
      console.error(error);
      toast.error("Failed to save changes.");
    }
  };

  // ==============================
  // RENDER
  // ==============================
  if (loading) return <h3>Loading...</h3>;
  if (!hasAccess) return <h2>Access Denied. SuperUser only.</h2>;

  return (
    <header className="cmn-header-main-frame">
    <div className="menu-background custom-scrollbar">
      {/* <CompanyManagementHeader title={activeTab} /> */}
       <div className="cmn-header-left-section">
        <h1 className="cmn-logo-text">{title || 'Company Management'}</h1>
      </div>
      <div className="cmn-header-right-section">
   
    
        <div className="cmn-datetime-wrapper">
          <div className="cmn-datetime-date-container">
            <span className="cmn-datetime-month">{currentDate}</span>
          </div>
          <div className="cmn-datetime-time-container">
            <span className="cmn-datetime-time">{currentTime}</span>
          </div>
        </div>
      </div>

      <div className="nav-bar-with-buttons">
        <CompanyManagementNavBar
          tabs={navTabs}
          activeTab={activeTab}
          onTabChange={(tab) => {
            if (tab !== "Position Management") {
              navigate("/companyManagement");
            } else {
              setActiveTab(tab);
            }
          }}
          tabWidths={tabWidths}
        />
      </div>

      <div >
          <button className="add-position-button" onClick={handleSave}>
            Save 
          </button>
        </div>
      

      

      <div className="content-container">
        <table className="position-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Branch</th>
              <th>Current Position</th>
              <th>Position Title</th>
            </tr>
          </thead>

          <tbody>
            {currentEmployees.length === 0 ? (
              <tr>
        
                  No users found.
            
              </tr>
            ) : (
              currentEmployees.map((employee) => (
                <tr key={employee.employeeId}>
                  <td>
                    {employee.name} {employee.surname}
                  </td>
                  <td>{employee.branch || "N/A"}</td>
                  <td>
                    {positionMap[employee.positionId] || "N/A"}
                  </td>
                  <td>
                    {positionMap[employee.positionId] || "N/A"}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* ==============================
           PAGINATION
         ============================== */}
      <div className="pagination-wrapper">
        <div className="pagination-left">
          <span className="pagination-range">
            <strong className="pagination-bold">
              {employees.length === 0 ? 0 : indexOfFirstItem + 1} -{" "}
              {Math.min(indexOfLastItem, employees.length)}
            </strong>{" "}
            of {employees.length}
          </span>

          <div
            className="per-page-box"
            onClick={() => setShowPageOptions(!showPageOptions)}
          >
            <span className="per-page-number">{itemsPerPage}</span>
            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown"
              className="dropdown-icon"
            />
            {showPageOptions && (
              <ul className="per-page-dropdown">
                {pageOptions.map((option) => (
                  <li
                    key={option}
                    className="per-page-option"
                    onClick={(e) => {
                      e.stopPropagation();
                      setItemsPerPage(option);
                      setShowPageOptions(false);
                      setCurrentPage(1);
                    }}
                  >
                    {option}
                  </li>
                ))}
              </ul>
            )}
          </div>
          <span className="per-page-label">Per page</span>
        </div>

        <div className="pagination-right">
          <img
            src="/images/arrow_drop_down_circle.png"
            alt="Previous"
            className="pagination-arrow prev"
            onClick={handlePrev}
            style={{
              transform: "rotate(90deg)",
              cursor: currentPage > 1 ? "pointer" : "not-allowed",
              opacity: currentPage > 1 ? 1 : 0.4,
            }}
          />

          <div className="page-numbers">
            {Array.from({ length: totalPages }, (_, i) => (
              <button
                key={i + 1}
                className={`page-number ${
                  currentPage === i + 1 ? "active-page" : ""
                }`}
                onClick={() => handlePageClick(i + 1)}
              >
                {i + 1}
              </button>
            ))}
          </div>

          <img
            src="/images/arrow_drop_down_circle.png"
            alt="Next"
            className="pagination-arrow next"
            onClick={handleNext}
            style={{
              transform: "rotate(-90deg)",
              cursor: currentPage < totalPages ? "pointer" : "not-allowed",
              opacity: currentPage < totalPages ? 1 : 0.4,
            }}
          />

          <div className="pagination-info">
            {employees.length} Users @ Singular
          </div>
        </div>
      </div>
    </div>
    </header>
  );
};

export default ManageUserPositions;