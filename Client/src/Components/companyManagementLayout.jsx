import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import CompanyManagementNavBar from "./CompanyManagementNavBar";
import PositionManagement from "./PositionManagement";
import CompanyManagementHeader from "./companyManagement/companyManagementHeader";
import CompanyManagementUI from "./companyManagement/companyManagementUI";

const navTabs = [
  "Tax Table Management",
  "Upload TAX Tables",
  "Company Details",
  "Leave Management",
  "Position Management",
  "Manage Companies",
  "Salary Budgets"
];

const tabWidths = [168, 133, 122, 134, 154, 125, 120];

const CompanyManagementLayout = ({
  companies,
  selectedCompanyId,
  viewedCompanyDetails,
  handlers,
  config
}) => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState(config?.initialTab || "Company Details");

  // Get the header title based on active tab
  const getHeaderTitle = () => {
    switch (activeTab) {
      case "Position Management":
        return "Position Management";
      case "Company Details":
        return "Company Details";
      default:
        return activeTab;
    }
  };

  return (
    <div className="edit-employee-background">
      {/* Shared Header */}
      <CompanyManagementHeader title={getHeaderTitle()} />

      {/* Shared Tabs */}
      <CompanyManagementNavBar
        tabs={navTabs}
        activeTab={activeTab}
        onTabChange={setActiveTab}  // Passing the setter directly
        tabWidths={tabWidths}
      />

      {/* Conditional Add Button */}
      {activeTab === "Position Management" && (
        <div className="position-header-action-bar">
          <button
            className="add-position-button"
            onClick={() => navigate("/addpositionmanagement")}
          >
            Add Position
          </button>
        </div>
      )}

      {/* Main Sections */}
      <main className="cm-sections-container">
        {activeTab === "Position Management" ? (
          <PositionManagement activeTab={activeTab} setActiveTab={setActiveTab} />
        ) : (
          <CompanyManagementUI
            companies={companies}
            selectedCompanyId={selectedCompanyId}
            viewedCompanyDetails={viewedCompanyDetails}
            handlers={handlers}
            config={{ initialTab: activeTab }}
          />
        )}
      </main>
    </div>
  );
};

export default CompanyManagementLayout;
