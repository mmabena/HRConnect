import React, { useState, useEffect } from "react";
import "./CompanyList.css";
import { Link } from "react-router-dom";
import { Plus } from "lucide-react";
import { fetchAllCompanies } from "../api/Company";
import AddCompanyModal from "../Components/companyManagement/AddCompanyModal.jsx";

const CompanyList = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const [companies, setCompanies] = useState([]);
  const [showAddModal, setShowAddModal] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadCompanies = async () => {
      try {
        setLoading(true);
        const data = await fetchAllCompanies();
        setCompanies(data);
      } catch (err) {
        setError("Failed to load companies");
      } finally {
        setLoading(false);
      }
    };

    loadCompanies();
  }, []);

  const handleAddCompanyClick = () => {
    setShowAddModal(true);
  };

  const filteredCompanies = companies.filter((company) =>
    company.companyName?.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  return (
    <div className="menu-background">
      <div className="CL-menu-bar"></div>

      <div className="CL-wrapper-container">
        {/* Header Section */}
        <div className="CL-company-header">
          <div className="CL-company-header-left">
            <h1 className="CL-company-title-main">Company Management</h1>
          </div>

          <div className="CL-company-header-right">
            {/* (Optional future actions area) */}
          </div>
        </div>

        {/* Navigation Tabs */}
        <div className="CL-company-nav">
          <span className="CL-nav-item">Tax Table Management</span>
          <span className="CL-nav-item">Leave Management</span>
          <span className="CL-nav-item">Position Management</span>
          <span className="CL-nav-item active">Company Details</span>
          <span className="CL-nav-item">Salary Budgets</span>
        </div>

        {/* Top Controls */}
        <div className="CL-company-header-row">
          <div className="CL-search-bar-container">
            <img
              src="/images/search.svg"
              alt="Search"
              className="CL-search-icon"
            />
            <input
              type="text"
              className="CL-search-input"
              placeholder="Search companies..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>

          <button
            className="CL-add-company-button"
            onClick={handleAddCompanyClick}
          >
            <Plus size={20} className="CL-add-icon" />
            <span>Add Company</span>
          </button>
        </div>

        {/* Table */}
        <div className="CL-content-container">
          {/* Table Top Bar */}
          <div className="CL-table-top-bar">
            <span className="CL-table-title">Registered Companies</span>
          </div>

          <div className="CL-company-table-grid">
            {/* Headers */}
            <div className="CL-table-header">Company</div>
            <div className="CL-table-header">Reg Number</div>
            <div className="CL-table-header">UIF Number</div>
            <div className="CL-table-header">VAT Number</div>
            <div className="CL-table-header action-header">Actions</div>

            {/* Loading */}
            {loading && (
              <div className="CL-no-data-row">Loading companies...</div>
            )}

            {/* Error */}
            {error && <div className="CL-no-data-row">{error}</div>}

            {/* No Data */}
            {!loading && !error && filteredCompanies.length === 0 && (
              <div className="CL-no-data-row">No companies found.</div>
            )}

            {/* Rows */}
            {!loading &&
              !error &&
              filteredCompanies.map((company) => (
                <React.Fragment key={company.companyId}>
                  {/* Company */}
                  <div className="CL-table-cell company-name-cell">
                    <div className="CL-initials-circle">
                      {company.companyName
                        ?.split(" ")
                        .map((word) => word[0] || "")
                        .slice(0, 2)
                        .join("")
                        .toUpperCase()}
                    </div>

                    <div className="CL-company-name-text">
                      <div className="CL-company-title">
                        {company.companyName}
                      </div>
                      <div className="CL-company-sub">
                        {company.contactNumber}
                      </div>
                    </div>
                  </div>

                  <div className="CL-table-cell">
                    {company.registrationNumber}
                  </div>
                  <div className="CL-table-cell">{company.uifNumber}</div>
                  <div className="CL-table-cell">
                    {company.vatNumber || "—"}
                  </div>

                  {/* Actions */}
                  <div className="CL-table-cell view-edit-cell">
                    <Link
                      to={`/company/${company.companyId}`}
                      state={company}
                      className="CL-edit-btn"
                    >
                      View | Edit
                    </Link>
                  </div>
                </React.Fragment>
              ))}
          </div>
        </div>
      </div>
      {showAddModal && (
        <div
          className="add-employee-overlay"
          onClick={() => setShowAddModal(false)}
        >
          <div onClick={(e) => e.stopPropagation()}>
            <AddCompanyModal closeModal={() => setShowAddModal(false)} />
          </div>
        </div>
      )}
    </div>
  );
};

export default CompanyList;
