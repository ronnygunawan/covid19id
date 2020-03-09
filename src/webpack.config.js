"use strict";
var path = require("path");

module.exports = {
	mode: "production",
	entry: {
		app: path.resolve("./ts/index.tsx")
	},
	output: {
		filename: "[name].bundle.js",
		path: path.resolve("../docs/js/")
	},
	resolve: {
		extensions: [".ts", ".tsx", ".js", ".jsx"]
	},
	module: {
		rules: [
			{
                test: /\.(t|j)s(x?)$/,
				exclude: /node_modules/,
                use: [
					{
						loader: 'ts-loader'
					}
				]
			},
			{ enforce: "pre", test: /\.js$/, loader: "source-map-loader" }
		]
	},
	externals: {
		"react": "React",
		"react-dom": "ReactDOM",
		"jquery": "jQuery"
	},
	devtool: "source-map"
};