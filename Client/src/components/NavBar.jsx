import { NavLink } from "react-router-dom";
import "./NavBar.css";

export default function NavBar() {

  return (
    <nav className="neo-nav">
          <div className="nav-shell">
              
        <div className="nav-links">
                <NavLink to="/taxTableManagement" className="nav-item">Tax Table Management</NavLink>
                <NavLink to="/leaveManagement" className="nav-item">Leave Management</NavLink>
                <NavLink to="/positionManagement" className="nav-item">Position Management</NavLink>
                <NavLink to="/companyDetails" className="nav-item">Company Details</NavLink>
                <NavLink to="/salaryBudgets" className="nav-item">Salary Budgets</NavLink>
        </div>
      </div>
    </nav>
  );
}
