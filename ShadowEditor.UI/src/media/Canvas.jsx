import './css/Canvas.css';
import classNames from 'classnames/bind';
import PropTypes from 'prop-types';

/**
 * 画布
 * @author tengge / https://github.com/tengge1
 * @property {String} className 样式类
 * @property {Object} style 样式
 */
class Canvas extends React.Component {
    render() {
        const { className, style } = this.props;

        return <canvas className={classNames('Canvas', className)} style={style}></canvas>;
    }
}

Canvas.propTypes = {
    className: PropTypes.string,
    style: PropTypes.object,
};

export default Canvas;