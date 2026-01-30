import '../../MenuBar.css'; 

const CompanyManagementNavBar = ({
  tabs,
  activeTab,
  onTabChange,
  tabWidths = []
}) => {
  return (
    <nav className="cm-navbar-container" role="navigation" aria-label="Company Management Navigation">
      {tabs.map((tabItem, index) => {
     
        const widthClass = tabWidths[index]
          ? `cm-navbar-tab-button--width-${tabWidths[index]}`
          : '';

        return (
          <button
            key={tabItem}
            className={`cm-navbar-tab-button ${widthClass} ${
              activeTab === tabItem ? 'cm-navbar-tab-button--active' : ''
            }`}
            onClick={() => onTabChange(tabItem)}
            aria-selected={activeTab === tabItem}
            role="tab"
          >
            {tabItem}
          </button>
        );
      })}
    </nav>
  );
};

export default CompanyManagementNavBar;