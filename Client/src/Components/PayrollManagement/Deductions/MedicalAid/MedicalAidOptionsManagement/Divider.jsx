import React from 'react';
import './Divider.css';

const Divider = ({dashed}) => {
    const className = dashed ? 'divider divider-dashed' : 'divider';
    return <hr className={className}/>
}

export default Divider;