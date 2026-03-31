import React, { useState, useEffect } from "react";
import CompanyManagementNavBar from "../../../Components/companyManagement/companyManagementNavBar";
import { useNavigate, useLocation } from "react-router-dom";
import { editEmployee } from "../../../api/Employee";
import api from "../../../api/api";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const ManageUserPositions = ({ title }) => {
  const [employees, setEmployees] = useState([]);
  const [positions, setPositions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);
  const [currentTime, setCurrentTime] = useState("");
  const [currentDate, setCurrentDate] = useState("");
  const [selectedEmployees, setSelectedEmployees] = useState({});
  const [selectedPosition, setSelectedPosition] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);
  const [showPageOptions, setShowPageOptions] = useState(false);
  const [activeTab, setActiveTab] = useState("Position Management");

  const navigate = useNavigate();
  const location = useLocation();
  const { currentPositionTitle, newPositionTitle } = location.state || {};
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

  // ----------------------------
  // Date & Time
  // ----------------------------
  useEffect(() => {
    const updateDateTime = () => {
      const now = new Date();
      const month = now.toLocaleDateString("en-ZA", { month: "short" });
      const day = now.toLocaleDateString("en-ZA", { day: "2-digit" });
      const year = now.toLocaleDateString("en-ZA", { year: "numeric" });
      const time = now.toLocaleTimeString("en-ZA", {
        hour: "2-digit",
        minute: "2-digit",
        hour12: false,
      });
      setCurrentDate(`${month}. ${day}, ${year}`);
      setCurrentTime(time);
    };
    updateDateTime();
    const intervalId = setInterval(updateDateTime, 60000);
    return () => clearInterval(intervalId);
  }, []);

  // ----------------------------
  // Initialization & Auth
  // ----------------------------
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

        // Fetch employees and positions
        const [employeesRes, positionsRes] = await Promise.all([
          api.get("/employee"),
          api.get("/positions"),
        ]);

        const allEmployees = employeesRes.data || [];
        const allPositions = positionsRes.data || [];

        setPositions(allPositions);

        // Find current position
        const matchedCurrent = allPositions.find(
          (pos) => pos.positionTitle === currentPositionTitle,
        );

        // Filter employees by CURRENT position
        const filteredEmployees = matchedCurrent
          ? allEmployees.filter(
              (emp) =>
                Number(emp.positionId) === Number(matchedCurrent.positionId),
            )
          : [];

        setEmployees(filteredEmployees);

        // Find new position after positions are loaded
        const matchedNew = allPositions.find(
          (pos) => pos.positionTitle === newPositionTitle,
        );

        if (matchedNew) {
          setSelectedPosition(String(matchedNew.positionId));
        } else {
          const firstOther = allPositions.find(
            (pos) => pos.positionId !== matchedCurrent?.positionId,
          );
          setSelectedPosition(firstOther ? String(firstOther.positionId) : "");
        }
      } catch (error) {
        console.error("Initialization error:", error);
        toast.error("Failed to load data.");
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, [currentPositionTitle, newPositionTitle]);

  // ----------------------------
  // Checkbox Handlers
  // ----------------------------
  const handleCheckboxChange = (employeeId) => {
    setSelectedEmployees((prev) => ({
      ...prev,
      [employeeId]: !prev[employeeId],
    }));
  };

  const handleSelectAll = () => {
    const allSelected = currentEmployees.every(
      (emp) => selectedEmployees[emp.employeeId],
    );
    const newSelection = { ...selectedEmployees };
    currentEmployees.forEach((emp) => {
      newSelection[emp.employeeId] = !allSelected;
    });
    setSelectedEmployees(newSelection);
  };

  // ----------------------------
  // Position Mapping
  // ----------------------------
  const positionMap = Object.fromEntries(
    positions.map((pos) => [pos.positionId, pos.positionTitle]),
  );

  const handlePositionChange = (e) => {
    setSelectedPosition(e.target.value);
  };

  // ----------------------------
  // Pagination
  // ----------------------------
  const totalPages = Math.ceil(employees.length / itemsPerPage);
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentEmployees = employees.slice(indexOfFirstItem, indexOfLastItem);

  const handlePrev = () =>
    currentPage > 1 && setCurrentPage((prev) => prev - 1);
  const handleNext = () =>
    currentPage < totalPages && setCurrentPage((prev) => prev + 1);
  const handlePageClick = (num) => setCurrentPage(num);

  // ----------------------------
  // Save Changes (Update Position)
  // ----------------------------
  const handleSave = async () => {
    const selectedIds = Object.keys(selectedEmployees).filter(
      (id) => selectedEmployees[id],
    );

    if (!selectedIds.length) {
      toast.error("Please select at least one employee.");
      return;
    }

    if (!selectedPosition) {
      toast.error("Please select a position.");
      return;
    }

    try {
      const updatePromises = selectedIds
        .map((employeeId) => {
          const emp = employees.find(
            (e) => e.employeeId.toString() === employeeId,
          );
          if (!emp) return null;

          //Send full employee object with updated positionId
          const updatedEmp = {
            ...emp,
            positionId: Number(selectedPosition),
            nationality: emp.nationality || "Not specified", // required field
            title: emp.title || "Mr/Ms", // fill if missing
            name: emp.name || "",
            surname: emp.surname || "",
            idNumber: emp.idNumber || "",
            passportNumber: emp.passportNumber || "",
            gender: emp.gender || "Male",
            contactNumber: emp.contactNumber || "",
            taxNumber: emp.taxNumber || "",
            email: emp.email || "",
            physicalAddress: emp.physicalAddress || "",
            city: emp.city || "",
            zipCode: emp.zipCode || "",
            hasDisability: emp.hasDisability || false,
            disabilityDescription: emp.disabilityDescription || "",
            dateOfBirth:
              emp.dateOfBirth || new Date().toISOString().split("T")[0],
            startDate: emp.startDate || new Date().toISOString().split("T")[0],
            branch: emp.branch || "",
            monthlySalary: emp.monthlySalary || 0,
            employmentStatus: emp.employmentStatus || "Permanent",
            careerManagerID: emp.careerManagerID || "",
            profileImage: emp.profileImage || "",
          };

          return editEmployee(emp.employeeId, updatedEmp);
        })
        .filter(Boolean);

      await Promise.all(updatePromises);

      toast.success("Positions updated successfully.");

      setEmployees((prev) =>
        prev.map((emp) =>
          selectedIds.includes(emp.employeeId.toString())
            ? { ...emp, positionId: Number(selectedPosition) }
            : emp,
        ),
      );

      setSelectedEmployees({});
    } catch (error) {
      console.error(
        "Failed to update positions:",
        error.response || error.message,
      );
      toast.error("Failed to save changes. Check console for details.");
    }
  };

  if (loading) return <h3>Loading...</h3>;
  if (!hasAccess) return <h2>Access Denied. SuperUser only.</h2>;

  return (
    <header className="cmn-header-main-frame">
      <div className="menu-background custom-scrollbar">
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

        <CompanyManagementNavBar
          tabs={navTabs}
          activeTab={activeTab}
          onTabChange={(tab) => {
            if (tab !== "Position Management") navigate("/companyManagement");
            else setActiveTab(tab);
          }}
          tabWidths={tabWidths}
        />

        {/* Move Users Dropdown */}
        <div className="move-users-wrapper">
          <div className="move-users-row">
            <p className="move-users-label">Move users to:</p>
            <div className="apm-dropdown-wrapper">
              <select
                className="apm-input select-dropdown"
                value={selectedPosition}
                onChange={handlePositionChange}
                required
              >
                {positions.map((position) => (
                  <option
                    key={position.positionId}
                    value={String(position.positionId)}
                  >
                    {position.positionTitle}
                  </option>
                ))}
              </select>
              <img
                src="/images/arrow_drop_down_circle.png"
                alt="Dropdown Icon"
                className="apm-dropdown-icon-new"
              />
            </div>
          </div>
          <button className="save-position-button" onClick={handleSave}>
            Save
          </button>
        </div>

        {/* Employee Table */}
        <div className="manage-positions"></div>
        <table className="positions-table">
          <thead>
            <tr>
              <th>
                <div className="checkbox-cell">
                  <input
                    type="checkbox"
                    checked={
                      currentEmployees.length > 0 &&
                      currentEmployees.every(
                        (emp) => selectedEmployees[emp.employeeId],
                      )
                    }
                    onChange={handleSelectAll}
                  />
                  <span>Name</span>
                </div>
              </th>

              <th>Branch</th>
              <th>Current Position</th>
              <th>Position Title</th>
            </tr>
          </thead>

          <tbody>
            {currentEmployees.length === 0 ? (
              <tr>
                <td>No users found.</td>
              </tr>
            ) : (
              currentEmployees.map((employee) => (
                <tr key={employee.employeeId}>
                  <td>
                    <div className="checkbox-cell">
                      <input
                        type="checkbox"
                        checked={!!selectedEmployees[employee.employeeId]}
                        onChange={() =>
                          handleCheckboxChange(employee.employeeId)
                        }
                      />
                      <span>
                        {employee.name} {employee.surname}
                      </span>
                    </div>
                  </td>

                  <td>{employee.branch || "N/A"}</td>
                  <td>{positionMap[employee.positionId] || "N/A"}</td>
                  <td>{positionMap[selectedPosition] || "-"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {/* Pagination */}
        {employees.length > itemsPerPage && (
          <div className="pagination-placeholder">
            <div className="pagination-wrapper">
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
                    cursor:
                      currentPage < totalPages ? "pointer" : "not-allowed",
                    opacity: currentPage < totalPages ? 1 : 0.4,
                  }}
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </header>
  );
};

export default ManageUserPositions;