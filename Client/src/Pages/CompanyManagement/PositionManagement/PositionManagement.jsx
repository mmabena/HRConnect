import React, { useState, useEffect } from "react";
import CompanyManagementHeader from "../../../Components/CompanyManagement/companyManagementHeader";
import CompanyManagementNavBar from "../../../Components/CompanyManagement/companyManagementNavBar";
import { useNavigate } from "react-router-dom";
import api from "../../../api/api"; // use axios instance
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";
import AddPositionManagement from "../../../Components/CompanyManagement/PositionManagement/AddPositionManagment";

const PositionManagement = () => {
  const [positions, setPositions] = useState([]);
  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);

  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);
  const [showPageOptions, setShowPageOptions] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);

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
  const tabWidths = [168, 133, 122, 134, 154, 125, 120];

  // --------------------------
  // INITIALIZATION + AUTH
  // --------------------------
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

        // Fetch positions, job grades, and occupational levels
        const [positionsRes, gradesRes, levelsRes] = await Promise.all([
          api.get("/positions"),
          api.get("/jobgrades"),
          api.get("/occupationallevels"),
        ]);

        setPositions(positionsRes.data);
        setJobGrades(gradesRes.data);
        setOccupationalLevels(levelsRes.data);
      } catch (error) {
        console.error("Initialization error:", error);
        toast.error("Failed to load data. Unauthorized.");
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  // --------------------------
  // PAGINATION LOGIC
  // --------------------------
  const totalPages = Math.ceil(positions.length / itemsPerPage);
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentPositions = positions.slice(indexOfFirstItem, indexOfLastItem);

  const handlePrev = () =>
    currentPage > 1 && setCurrentPage((prev) => prev - 1);
  const handleNext = () =>
    currentPage < totalPages && setCurrentPage((prev) => prev + 1);
  const handlePageClick = (num) => setCurrentPage(num);
  const handleAddPositionClick = () => setShowAddModal;

  const jobGradeMap = Object.fromEntries(
    jobGrades.map((grade) => [grade.jobGradeId, grade.name]),
  );
  const occupationalLevelMap = Object.fromEntries(
    occupationalLevels.map((level) => [
      level.occupationalLevelId,
      level.description,
    ]),
  );

  // --------------------------
  // RENDER
  // --------------------------
  if (loading) return <h3>Loading...</h3>;
  if (!hasAccess) return <h2>Access Denied. SuperUser only.</h2>;

  return (
    <div className="menu-background custom-scrollbar">
      <CompanyManagementHeader title={activeTab} />

      <div className="nav-bar-with-button">
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

        {activeTab === "Position Management" && (
          <button
            className="add-position-button"
            onClick={handleAddPositionClick}
          >
            Add New Position
          </button>
        )}
      </div>

      <div className="content-container">
        <table className="position-table">
          <thead>
            <tr>
              <th>Position Title</th>
              <th>Position Grade</th>
              <th>Occupational Description</th>
              <th>Effective Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {currentPositions.length === 0 ? (
              <tr>
                <td colSpan="5" style={{ textAlign: "center" }}>
                  No positions found.
                </td>
              </tr>
            ) : (
              currentPositions.map((position) => (
                <tr key={position.positionId}>
                  <td>{position.positionTitle}</td>
                  <td>{position.jobGrade?.name || "N/A"}</td>
                  <td>{position.occupationalLevel?.description || "N/A"}</td>
                  <td>
                    {new Date(position.createdDate).toLocaleDateString(
                      "en-GB",
                      {
                        day: "numeric",
                        month: "long",
                        year: "numeric",
                      },
                    )}
                  </td>
                  <td>
                    <div className="edit-button-group">
                      <button
                        className="text-button"
                        onClick={() =>
                          navigate(
                            `/editPositionManagement/${position.positionId}`,
                          )
                        }
                      >
                        Edit
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* PAGINATION */}
      <div className="pagination-wrapper">
        <div className="pagination-left">
          <span className="pagination-range">
            <strong className="pagination-bold">
              {positions.length === 0 ? 0 : indexOfFirstItem + 1} -{" "}
              {Math.min(indexOfLastItem, positions.length)}
            </strong>{" "}
            of {positions.length}
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
                className={`page-number ${currentPage === i + 1 ? "active-page" : ""}`}
                onClick={() => handlePageClick(i + 1)}
                aria-label={`Go to page ${i + 1}`}
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
            {positions.length} Positions @ Singular
          </div>
          {showAddModal && (
            <AddPositionManagement
              isOpen={showAddModal}
              onClose={() => setShowAddModal(false)}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default PositionManagement;
