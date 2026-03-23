import React, { useState, useEffect } from "react";
import { fetchAllEmployees } from "../../api/Employee";
import { Link, useLocation, useNavigate } from "react-router-dom";
import AddEmployeeModal from "../../Components/EmployeeManagement/AddEmployeeModal";

import useEmployees from "../../hooks/useEmployees";
import useEmployeeFilter from "../../hooks/useEmployeeFilter";
import useEmpPagination from "../../hooks/useEmpPagination";
import useDropdown from "../../hooks/useDropdown";
import useInitialColors from "../../hooks/useInitialColors";


import "../../Components/MenuBar/MenuBar.css";
import "./EmployeeList.css";

const EmployeeList = () => {
  const tabs = [
    { label: "All staff", value: "All" },
    { label: "Johannesburg", value: "Johannesburg" },
    { label: "Cape Town", value: "CapeTown" },
    { label: "UK(London)", value: "UK" },
  ];

  const [selectedTab, setSelectedTab] = useState("All");
  const location = useLocation();
  const navigate = useNavigate();
  const [showAddModal, setShowAddModal] = useState(false);
  const { employees, loading, error } = useEmployees(location.key);
  const [searchQuery, setSearchQuery] = useState("");
  

  const filteredEmployees = useEmployeeFilter(
    employees,
    selectedTab,
    searchQuery
  );

  /// </summary>
  /// Pagination states
  /// </summary>
  const {
    activePage,
    setActivePage,
    itemsPerPage,
    setItemsPerPage,
    totalPages,
    indexOfFirstItem,
    indexOfLastItem,
    currentItems,
  } = useEmpPagination(filteredEmployees);

  /// </summary>
  ///colors for the initial circles
  /// </summary>

  const { dropdownOpen, toggleDropdown, closeDropdown } = useDropdown();

  const { COLORS } = useInitialColors();

  const handleItemsPerPageChange = (option) => {
    setItemsPerPage(option);
    closeDropdown();
    setActivePage(1);
  };

  const handleAddEmployeeClick = () => {
  setShowAddModal(true);
};

  return (
    <div className="menu-background">
      <div className="menu-bar"></div>

      <div className="wrapper-container">
        <div className="singular-staff-heading-container">Singular Staff</div>

        <div className="employee-list-heading-row">
          {tabs.map((tab) => (
            <div
              key={tab.value}
              className={`heading-item ${
                selectedTab === tab.value ? "selected" : ""
              }`}
              onClick={() => {
                setSelectedTab(tab.value); 
                setActivePage(1); 
              }} 
            >
              {tab.label}
            </div>
          ))}
          <div className="right-controls">
            <div className="heading-item filter-search-wrapper">
              <span className="filter-label">Filter</span>
              <div className="search-bar-container">
                <img
                  src="/images/menu.svg"
                  alt="Left Icon"
                  className="search-icon"
                />
                <div className="input-wrapper">
                  <input
                    type="text"
                    className="search-input"
                    placeholder="Search text"
                    value={searchQuery}
                    onChange={(e) => {
                      setSearchQuery(e.target.value);
                      /// </summary>
                      setActivePage(1); /// reset page on search
                      /// </summary>
                    }}
                  />
                </div>
                <img
                  src="/images/search.svg"
                  alt="Right Icon"
                  className="search-icon"
                />
              </div>
            </div>
            <button
              className="add-employee-button"
              onClick={handleAddEmployeeClick}
            >
              Add Employee
            </button>
          </div>
        </div>

        <div className="content-container">
          <div className="employee-table-grid">
            <div className="table-header">Employee Code</div>
            <div className="table-header">Name & Surname</div>
            <div className="table-header">Job Title</div>
            <div className="table-header">Contact Number</div>
            <div className="table-header">Email</div>
            <div className="table-header">Employment Status</div>
            <div className="table-header">Branch</div>
            <div className="table-header-action">Action</div>

            {loading && <div className="loading-row">Loading employees...</div>}

            {error && <div className="error-row">{error}</div>}

            {!loading && !error && currentItems.length === 0 && (
              <div className="no-data-row">No employees found.</div>
            )}

            {!loading &&
              !error &&
              currentItems.map((emp, index) => (
                <React.Fragment key={emp.employeeId}>
                  <div className="table-cell">{emp.employeeId}</div>

                  <div className="table-cell name-surname-cell">
                    <div
                      className={`initials-circle ${
                        COLORS[index % COLORS.length]
                      }`}
                    >
                      {(
                        emp.initials ||
                        `${(emp.name || "").charAt(0)}${(
                          emp.surname || ""
                        ).charAt(0)}`
                      ).toUpperCase()}
                    </div>

                    <span className="name-text">{`${emp.name} ${emp.surname}`}</span>
                  </div>

                  <div className="table-cell">{emp.positionTitle}</div>
                  <div className="table-cell">{emp.contactNumber}</div>
                  <div className="table-cell">{emp.email}</div>
                  <div className="table-cell">{emp.employmentStatus}</div>
                  <div className="table-cell">{emp.branch}</div>
                  <div className="table-cell view-edit-cell">
                    <Link
                      to={`/editEmployee/${emp.employeeId}`}
                      state={emp}
                      className="edit-btn"
                    >
                      Edit
                    </Link>
                  </div>
                </React.Fragment>
              ))}
          </div>
        </div>
      </div>

      <div className="pagination-container">
        <div className="pagination-left-section">
          <span className="pagination-range">
            <strong className="range-bold">
              {indexOfFirstItem + 1} -{" "}
              {Math.min(indexOfLastItem, filteredEmployees.length)}
            </strong>{" "}
            of {filteredEmployees.length}
          </span>

          <div className="per-page-box" onClick={toggleDropdown}>
            <span className="per-page-number">{itemsPerPage}</span>
            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown"
              className="dropdown-icon"
            />
            {dropdownOpen && (
              <div className="dropdown-options">
                {[10].map((option) => (
                  <div
                    key={option}
                    className="dropdown-option"
                    onClick={() => handleItemsPerPageChange(option)}
                  >
                    {option}
                  </div>
                ))}
              </div>
            )}
          </div>

          <span className="per-page-label">Per page</span>
        </div>

        <div className="pagination-right-section">
          <div className="pagination-controls">
            {/* Go to First Page */}
            <img
              src="/images/arrow_drop_down_circle.png"
              alt="First"
              className={`pagination-arrow ${activePage === 1 ? "disabled" : ""}`}
              onClick={() => activePage > 1 && setActivePage(1)}
            />

            {/* Go to Previous Page */}
            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Previous"
              className={`pagination-arrow ${activePage === 1 ? "disabled" : ""}`}
              onClick={() => activePage > 1 && setActivePage(activePage - 1)}
            />

            {/* Page numbers remain the same */}
            <div className="page-count">
              {Array.from({ length: totalPages || 1 }, (_, i) => {
                const pageNum = i + 1;
                return (
                  <button
                    key={pageNum}
                    onClick={() => setActivePage(pageNum)}
                    className={`page-number ${activePage === pageNum ? "active" : ""}`}
                  >
                    {pageNum}
                  </button>
                );
              })}
            </div>

            {/* Go to Next Page */}
            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Next"
              className={`pagination-arrow next ${
                activePage === totalPages ? "disabled" : ""
              }`}
              onClick={() =>
                activePage < totalPages && setActivePage(activePage + 1)
              }
            />

            {/* Go to Last Page */}
            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Last"
              className={`pagination-arrow next ${
                activePage === totalPages ? "disabled" : ""
              }`}
              onClick={() =>
                activePage < totalPages && setActivePage(totalPages)
              }
            />
          </div>
          <div className="employee-count">
            {`${filteredEmployees.length} Employees @ Singular`}
          </div>
        </div>
      </div>
      {showAddModal && (
        <div
          className="add-employee-overlay"
          onClick={() => setShowAddModal(false)}
        >
          <div onClick={(e) => e.stopPropagation()}>
            <AddEmployeeModal closeModal={() => setShowAddModal(false)} />
          </div>
        </div>
      )}
    </div>
  );
};

export default EmployeeList;