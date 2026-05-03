import { NavLink } from "react-router-dom";
import "./NavBar.css";

function PayrollNavbar() {
    return(
        <nav className="neo-nav">
          <div className="nav-shell">
              
        <div className="nav-links">
                <NavLink to="/earnings" className="nav-item">Earnings</NavLink>
                    <NavLink to="/deductions" className="nav-item">Deductions</NavLink>
                    <NavLink to="/company-contributions" className="nav-item">Company Contributions</NavLink>
                    <NavLink to="/bcea" className="nav-item">BCEA</NavLink>
                    <NavLink to="/oid" className="nav-item">OID</NavLink>
                    <NavLink to="/stock" className="nav-item">Stock</NavLink>
                    <NavLink to="/salarybenchmark" className="nav-item">Salary Benchmark</NavLink>
        </div>
      </div>
    </nav>
    );
}

export default PayrollNavbar;


