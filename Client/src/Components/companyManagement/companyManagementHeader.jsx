// companyManagementHeader.jsx
import { useState, useEffect } from 'react';
import '../../MenuBar.css';

function CompanyManagementHeader({ title }) {
  const [currentTime, setCurrentTime] = useState('');
  const [currentDate, setCurrentDate] = useState('');

  useEffect(() => {
    const updateDateTime = () => {
      const now = new Date();

      const month = now.toLocaleDateString('en-ZA', { month: 'short' });
      const day = now.toLocaleDateString('en-ZA', { day: '2-digit' });
      const year = now.toLocaleDateString('en-ZA', { year: 'numeric' });

      const time = now.toLocaleTimeString('en-ZA', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      });

      setCurrentDate(`${month}. ${day}, ${year}`);
      setCurrentTime(time);
    };

    updateDateTime();
    const intervalId = setInterval(updateDateTime, 60000);
    return () => clearInterval(intervalId);
  }, []);

  return (
    <header className="cm-header-main-frame">
      <div className="cm-header-left-section">
        <h1 className="cm-logo-text">{title || 'Company Management'}</h1>
      </div>
      <div className="cm-header-right-section">
        <div className="cm-notification-icon-wrapper">
          <img src="/images/icon.png" alt="Notifications" className="cm-notification-icon" />
        </div>
        <div className="cm-settings-icon-wrapper">
          <img src="/images/settingscm.png" alt="Settings" className="cm-settings-icon" />
        </div>
        <div className="cm-datetime-wrapper">
          <div className="cm-datetime-date-container">
            <span className="cm-datetime-month">{currentDate}</span>
          </div>
          <div className="cm-datetime-time-container">
            <span className="cm-datetime-time">{currentTime}</span>
          </div>
        </div>
      </div>
    </header>
  );
}

export default CompanyManagementHeader;
