import React, { useState, useEffect } from "react";
import { fetchAllEmployees } from "../Employee";
import { Link,useLocation } from "react-router-dom";

const EmployeeList = () => {
  const tabs = ["All staff", "Johannesburg", "Cape Town", "UK(London)"];
  const [selectedTab, setSelectedTab] = useState("All staff");

  const [currentTime, setCurrentTime] = useState("");
  const [currentDate, setCurrentDate] = useState("");
  const [employees, setEmployees] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
const location = useLocation();

  /// </summary>
  /// Pagination states
  /// </summary>
  const [itemsPerPage, setItemsPerPage] = useState(10);

  const [activePage, setActivePage] = useState(1);

  /// </summary>
  ///colors for the initial circles
  /// </summary>
  const COLORS = ["#006088", "#01A19A", "#AFBF74", "#002D40"];

  /// </summary>
  /// Array to recycle colors after array ends
  /// </summary>
  const getInitialColorByIndex = (index) => COLORS[index % COLORS.length];
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const handleDropdownToggle = () => {
    setDropdownOpen(!dropdownOpen);
  };

  const handleItemsPerPageChange = (option) => {
    setItemsPerPage(option);
    setDropdownOpen(false);
    setActivePage(1);
  };

  /// </summary>
  /// code for the date and time in the correct format
  /// </summary>
  useEffect(() => {
    const now = new Date();

    const timeOptions = { hour: "numeric", minute: "2-digit", hour12: true };
    const formattedTime = now.toLocaleTimeString("en-US", timeOptions);

    const dateOptions = { year: "numeric", month: "short", day: "numeric" };
    const formattedDate = now.toLocaleDateString("en-US", dateOptions);

    setCurrentTime(formattedTime);
    setCurrentDate(formattedDate);
  }, []);

  /// </summary>
  ///loading employees on page load
  /// </summary>
  useEffect(() => {
  const loadEmployees = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchAllEmployees();
      setEmployees(data);
      console.log("Employee fetched:",data);
    } catch (err) {
      setError("Failed to load employees.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  loadEmployees();
   /// </summary>
  /// Re-run every time the route changes
  /// </summary>
}, [location.key]);


  /// </summary>
  /// Filter by selected tab (location) and search query
  /// </summary>
  const filteredEmployees = employees.filter((emp) => {
    if (selectedTab !== "All staff") {
      const empDepartment = (emp.department || "")
        .toLowerCase()
        .replace(/\s+/g, "");
      const selected = selectedTab.toLowerCase().replace(/\s+/g, "");
      if (empDepartment !== selected) {
        return false;
      }
    }

    const search = searchQuery.toLowerCase();
    if (!search) return true;

    const fullName = `${emp.firstName} ${emp.lastName}`.toLowerCase();
    const jobTitle = emp.jobTitle?.toLowerCase() || "";
    const email = emp.email?.toLowerCase() || "";
    const id = emp.employeeNumber?.toString() || "";

    return (
      fullName.includes(search) ||
      jobTitle.includes(search) ||
      email.includes(search) ||
      id.includes(search)
    );
  });

  /// </summary>
  /// logic for pagination
  /// </summary>
  const totalPages = Math.ceil(filteredEmployees.length / itemsPerPage);
  const indexOfFirstItem = (activePage - 1) * itemsPerPage;
  const indexOfLastItem = indexOfFirstItem + itemsPerPage;
  const currentEmployees = filteredEmployees.slice(
    indexOfFirstItem,
    indexOfLastItem
  );

  return (
    <div className="edit-employee-background">
      <div className="menu-bar"></div>

      <div className="wrapper-container">
        <div className="singular-staff-heading-container">
          Singular Staff
          <div className="icon-wrapper">
            <img
              src="/images/notifications.png"
              alt="Notification Icon"
              className="heading-icon"
            />
            <img
              src="/images/Settings.png"
              alt="Settings Icon"
              className="heading-icon"
            />
            <div className="utility-box large-box">{currentDate}</div>
            <div className="utility-box small-box">{currentTime}</div>
          </div>
        </div>

        <div className="employee-list-heading-row">
          {tabs.map((tab) => (
            <div
              key={tab}
              className={`heading-item ${
                selectedTab === tab ? "selected" : ""
              }`}
              onClick={() => {
                setSelectedTab(tab); /// </summary>
                setActivePage(1); /// reset page on tab change
              }} /// </summary>
              style={{ cursor: "pointer" }}
            >
              {tab}
            </div>
          ))}

          <div className="heading-item filter-search-wrapper">
            <span className="filter-label">Filter</span>
            <div className="search-bar-container">
              {/* <img
                src="/images/Leading-icon.png"
                alt="Left Icon"
                className="search-icon"
              /> */}
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
                src="/images/Trailing-Elements.png"
                alt="Right Icon"
                className="search-icon"
              />
            </div>
          </div>
        </div>

        <div className="content-container">
          <div className="table-grid">
            <div className="table-header">ID</div>
            <div className="table-cell" style={{ backgroundColor: "#006088" }}>
              <img src="/images/Frame_287.png" alt="Block_logo" />
            </div>
            <div className="table-header">Name & Surname</div>
            <div className="table-header">Career Title</div>
            <div className="table-header">Phone Number</div>
            <div className="table-header">Email</div>
            <div className="table-header">Location</div>
            <div className="table-header"></div>

            {loading && (
              <div
                className="loading-row"
                style={{ gridColumn: "span 8", textAlign: "center" }}
              >
                Loading employees...
              </div>
            )}

            {error && (
              <div
                className="error-row"
                style={{
                  gridColumn: "span 8",
                  textAlign: "center",
                  color: "red",
                }}
              >
                {error}
              </div>
            )}

            {!loading && !error && currentEmployees.length === 0 && (
              <div
                className="no-data-row"
                style={{ gridColumn: "span 8", textAlign: "center" }}
              >
                No employees found.
              </div>
            )}

            {!loading &&
              !error &&
              currentEmployees.map((emp, index) => (
                <React.Fragment key={emp.employeeNumber}>
                  <div className="table-cell">{emp.employeeNumber}</div>

                  <div className="table-cell">
                    <img src="/images/Frame_287.png" alt="Block_logo" />
                  </div>

                  <div className="table-cell name-surname-cell">
                    <div
                      className="initials-circle"
                      style={{ backgroundColor: getInitialColorByIndex(index) }}
                    >
                      {(
                        emp.initials ||
                        `${(emp.firstName || "").charAt(0)}${(
                          emp.lastName || ""
                        ).charAt(0)}`
                      ).toUpperCase()}
                    </div>

                    <span className="name-text">{`${emp.firstName} ${emp.lastName}`}</span>
                  </div>

                  <div className="table-cell">{emp.jobTitle}</div>
                  <div className="table-cell">{emp.contactNumber}</div>
                  <div className="table-cell">{emp.email}</div>
                  <div className="table-cell">{emp.department || emp.city}</div>
                  <div className="table-cell view-edit-cell">
                    <Link
                      to={`/editEmployee/${emp.employeeNumber}`}
                      state={{ ...emp, readOnly: true }}
                      className="view-btn"
                    >
                      View
                    </Link>
                    <Link
                      to={`/editEmployee/${emp.employeeNumber}`}
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

          <div className="per-page-box" onClick={handleDropdownToggle}>
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
  onClick={() => activePage < totalPages && setActivePage(activePage + 1)}
/>

{/* Go to Last Page */}
<img
  src="/images/arrow_drop_down_circle.png"
  alt="Last"
  className={`pagination-arrow next ${
    activePage === totalPages ? "disabled" : ""
  }`}
  onClick={() => activePage < totalPages && setActivePage(totalPages)}
/>

          </div>

          <div className="employee-count">
            {`${filteredEmployees.length} Employees @ Singular`}
          </div>
        </div>
      </div>
    </div>
  );
};

export default EmployeeList;