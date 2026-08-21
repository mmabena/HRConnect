import { NavLink } from "react-router-dom";
import "./PayrollNavBar.css";

export default function PayrollNavBar() {
  return (
    <nav className="payroll-nav">
      <div className="nav-shell">
        <div className="nav-links">
          <NavLink to="/earnings" className="nav-item">Earnings</NavLink>
          <NavLink to="/deductions" className="nav-item">Deductions</NavLink>
          <NavLink to="/companyContributions" className="nav-item">Company Contributions</NavLink>
          <NavLink to="/bcea" className="nav-item">BCEA</NavLink>
          <NavLink to="/oid" className="nav-item">OID</NavLink>
          <NavLink to="/stock" className="nav-item">Stock</NavLink>
        </div>
      </div>
    </nav>
  );
}
