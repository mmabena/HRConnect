import { Link } from "react-router-dom";
import "./NavBar.css";

export default function NavBar() {

  return (
    <nav className="neo-nav">
          <div className="nav-shell">
              
        <div className="nav-links">
                <Link to="/taxTableManagement" className="nav-item">Tax Table Management</Link>
                <Link to="/leaveManagement" className="nav-item">Leave Management</Link>
                <Link to="/positionManagement" className="nav-item">Position Management</Link>
                <Link to="/companyDetails" className="nav-item">Company Details</Link>
                <Link to="/companyDetails" className="nav-item">Company Details</Link>
                <Link to="/salaryBudgets" className="nav-item">Salary Budgets</Link>
        </div>
      </div>
    </nav>
  );
}
