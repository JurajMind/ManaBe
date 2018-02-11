module.exports = {
    entry: "./Vue/index.js",
    output: {
        path: __dirname,
        filename: "./Vue/bundle.js"
    },
    module: {
        loaders: [
            { test: /\.vue$/, loader: 'vue-loader' }
        ]
    }
}