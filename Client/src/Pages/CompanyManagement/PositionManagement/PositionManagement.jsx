import { useState } from "react";
import { useNavigate } from "react-router-dom";
import NavBar from "../../../Components/NavBar.jsx";

import CompanyManagementNavBar from "../../../Components/CompanyManagement/companyManagementNavBar";
import AddPositionManagement from "../../../Components/CompanyManagement/PositionManagement/AddPositionManagment";
import EditPositionManagement from "../../../Components/CompanyManagement/PositionManagement/EditPositionManagement";
import ChangePositionManagement from "../../../Components/CompanyManagement/PositionManagement/ChangePositionManagement";

import usePositions from "../../../hooks/usePositions";
import usePagination from "../../../hooks/usePagination";
import useDateTime from "../../../hooks/useDateTime";
import { COMPANY_NAME } from "../../../config/companyConfig";

const PositionManagement = ({ title }) => {
  const navigate = useNavigate();

  // -------------------
  // Hooks
  // -------------------
  const { positions, loading, hasAccess } = usePositions();
  const { currentDate, currentTime } = useDateTime();

  const {
    currentPage,
    setCurrentPage,
    itemsPerPage,
    setItemsPerPage,
    totalPages,
    currentItems: currentPositions,
    handlePrev,
    handleNext,
    handlePageClick,
  } = usePagination(positions);

  // -------------------
  // Local UI State
  // -------------------
  const [showPageOptions, setShowPageOptions] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedPositionId, setSelectedPositionId] = useState(null);
  const [changeModalData, setChangeModalData] = useState(null);
  const [activeTab, setActiveTab] = useState("Position Management");

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

  // -------------------
  // Change Modal Controls
  // -------------------
  const openChangeModal = (data) => setChangeModalData(data);
  const closeChangeModal = () => setChangeModalData(null);

  // Destructure modal data for readability
  const {
    currentPosition,
    linkedEmployeesCount,
    attemptedTitle,
  } = changeModalData || {};

  // -------------------
  // Loading & Access
  // -------------------
  if (loading) return <h3>Loading...</h3>;
  if (!hasAccess) return <h2>Access Denied. SuperUser only.</h2>;

  return (
    <header className="cmn-header-main-frame">
      <div className="menu-background custom-scrollbar">
        
        {/* Header */}
        <div className="cmn-header-left-section">
          <h1 className="cmn-logo-text">{title || "Company Management"}</h1>
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

        {/* Navigation & Add Button */}
        <div className="nav-bar-with-buttons">
          <CompanyManagementNavBar
            tabs={navTabs}
            activeTab={activeTab}
            onTabChange={(tab) => {
              if (tab !== "Position Management") navigate("/companyManagement");
              else setActiveTab(tab);
            }}
            tabWidths={tabWidths}
          />

          {activeTab === "Position Management" && (
            <button
              className="add-positions-button"
              onClick={() => setShowAddModal(true)}
            >
              Add New Position
            </button>
          )}
        </div>

        {/* Positions Table */}
        <div className="manage-positions">
          <table className="positions-table">
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
                  <td colSpan={5}>No positions found.</td>
                </tr>
              ) : (
                currentPositions.map((position) => (
                  <tr key={position.positionId}>
                    <td>{position.positionTitle}</td>
                    <td>{position.jobGrade?.name || "N/A"}</td>

                    <td title={position.occupationalLevel?.description || "N/A"}>
                      {position.occupationalLevel?.description || "N/A"}
                    </td>

                    <td>
                      {new Date(position.createdDate).toLocaleDateString(
                        "en-GB",
                        {
                          day: "numeric",
                          month: "long",
                          year: "numeric",
                        }
                      )}
                    </td>

                    <td>
                      <div className="edit-button-group">
                        <button
                          className="text-button"
                          onClick={() => {
                            setSelectedPositionId(position.positionId);
                            setShowEditModal(true);
                          }}
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

        {/* Pagination */}
        <div className="pagination-wrapper">
          <div className="pagination-left">
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
                        setCurrentPage(1);
                        setShowPageOptions(false);
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
              className="pagination-arrow-prev"
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
              {positions.length} Positions @ {COMPANY_NAME}
            </div>
          </div>

          {/* Modals */}
          {showAddModal && (
            <AddPositionManagement
              isOpen={showAddModal}
              onClose={() => setShowAddModal(false)}
            />
          )}

          {showEditModal && (
            <EditPositionManagement
              id={selectedPositionId}
              isOpen={showEditModal}
              onClose={() => setShowEditModal(false)}
              onOpenChangeModal={openChangeModal}
            />
          )}

          {changeModalData && (
            <ChangePositionManagement
              isOpen
              onClose={closeChangeModal}
              currentPosition={currentPosition}
              linkedEmployeesCount={linkedEmployeesCount}
              attemptedTitle={attemptedTitle}
            />
          )}
        </div>
      </div>
    </header>
  );
};

export default PositionManagement;
