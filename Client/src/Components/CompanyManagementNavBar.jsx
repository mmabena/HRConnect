import React from 'react';


function CompanyManagementNavBar({ activeTab, setActiveTab }) {
  const navTabs = [
    "Tax Table Management",
    "Upload TAX Tables",
    "Company Details",
    "Leave Management", 
    "Position Management", 
    "Salary Budgets\u00A0"
  ];

  return (
    <nav className="cm-navbar-titles-frame" role="navigation" aria-label="Company Management Navigation">
      {navTabs.map((tabItem) => (
        <button
          key={tabItem}
          className={`cm-navbar-tab-button ${activeTab === tabItem ? 'cm-navbar-tab-button--active' : ''}`}
          onClick={() => setActiveTab(tabItem)}
          aria-selected={activeTab === tabItem}
          role="tab"
        >
          
          {tabItem}
        </button>
      ))}
    </nav>
  );
}

export default CompanyManagementNavBar;
