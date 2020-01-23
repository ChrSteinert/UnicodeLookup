// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

var path = require("path");

module.exports = {
    mode: "development",
    entry: "./src/Client/Client.fsproj",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js"
    },
    devtool: "source-map",
    devServer: {
        contentBase: "./public",
        port: 8080,
        hot: true,
        inline: true,
        proxy: {
            '/api/*': {
                target: 'http://localhost:8085',
                changeOrigin: true
            }
        }
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: {
                loader: "fable-loader"
            }
        }]
    }
};
