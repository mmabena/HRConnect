import React, { useState, useEffect } from "react";
import CompanyManagementHeader from "./companyManagement/companyManagementHeader";
import CompanyManagementNavBar from "./companyManagement/companyManagementNavBar";
import { useNavigate } from "react-router-dom";

import "../MenuBar.css";

const PositionManagement = () => {
  const [positions, setPositions] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);
  const [showPageOptions, setShowPageOptions] = useState(false);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("Position Management");

  const navigate = useNavigate();

  const pageOptions = [10, 15, 20, 25];
  const navTabs = [
    "Tax Table Management",
    "Upload TAX Tables",
    "Company Details",
    "Leave Management",
    "Position Management",
    "Manage Companies",
    "Salary Budgets",
  ];



  const tabWidths = [168, 133, 122, 134, 154, 125, 120];

  useEffect(() => {
    setLoading(true);
    fetch("http://localhost:5037/api/Positions")
      .then((res) => res.json())
      .then((data) => {
        setPositions(data);
        setLoading(false);
      })
      .catch((error) => {
        console.error("Error fetching positions:", error);
        setLoading(false);
      });
  }, []);

  const totalPages = Math.ceil(positions.length / itemsPerPage);
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentPositions = positions.slice(indexOfFirstItem, indexOfLastItem);

  const handleAddPositionClick = () => navigate("/addpositionmanagement");

  const handlePrev = () => {
    if (currentPage > 1) setCurrentPage((prev) => prev - 1);
  };

  const handleNext = () => {
    if (currentPage < totalPages) setCurrentPage((prev) => prev + 1);
  };

  const handlePageClick = (pageNum) => {
    setCurrentPage(pageNum);
  };

  return (
    <div className="edit-employee-background custom-scrollbar">
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
    <button className="add-position-button" onClick={handleAddPositionClick}>
      Add Position
    </button>
  )}
</div>



         <div className="pm-table-wrapper">
        <table className="position-table">
          <thead>
            <tr>
              <th>Description</th>
              <th>Effective Date</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan="3" style={{ textAlign: "center" }}>Loading...</td>
              </tr>
            ) : currentPositions.length === 0 ? (
              <tr>
                <td colSpan="3" style={{ textAlign: "center" }}>No positions found.</td>
              </tr>
            ) : (
              currentPositions.map((position) => (
                <tr key={position.positionId}>
                  <td>{position.positionTitle}</td>
                  <td>
                    {new Date(position.createdDate).toLocaleDateString("en-GB", {
                      day: "numeric",
                      month: "long",
                      year: "numeric",
                    })}
                  </td>
                  <td>
                    <div className="view-edit-button-group">
                      <button
                        className="text-button"
                        onClick={() =>
                          navigate(`/viewPositionManagement/${position.positionId}`)
                        }
                      >
                        View
                      </button>
                      <span>&nbsp;or&nbsp;</span>
                      <button
                        className="text-button"
                        onClick={() =>
                          navigate(`/editPositionManagement/${position.positionId}`)
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
  {/* Prev Arrow */}
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

  {/* Page Numbers */}
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

  {/* Next Arrow */}
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

  <div className="pagination-info">{positions.length} Positions @ Singular</div>
</div>

      </div>
    </div>
  );
};

export default PositionManagement;
